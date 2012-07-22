﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012.MISP
{
    public class ScriptError : Exception { public ScriptError(String msg) : base(msg) { } }

    public class TimeoutError : ScriptError
    {
        public TimeoutError() : base("Execution timed out.\nTreat this like a hard crash.\nSomething is wrong with your mud-lib!\nYour in-memory database may be corrupted.") { }
    }

    public class ScriptEvaluater
    {
        public Dictionary<String, Function> functions = new Dictionary<String, Function>();
        public Dictionary<String, Func<Context, ScriptObject, Object>> specialVariables
            = new Dictionary<string, Func<Context, ScriptObject, object>>();
        private Random random = new Random();

        public static T ArgumentType<T>(Object obj) where T : class
        {
            if (obj == null)
                throw new ScriptError("Expecting argument of type " + typeof(T) + ", got null. ");
            var r = obj as T;
            if (r == null)
                throw new ScriptError("Function argument is the wrong type. Expected type "
                    + typeof(T) + ", got " + obj.GetType() + ". ");
            return r;
        }

        public static void ArgumentCount(int c, ScriptList arguments)
        {
            if (c != arguments.Count) throw new ScriptError("Function expects " + c + " arguments. It received " +
                arguments.Count + ".");
        }

        public static void ArgumentCountOrGreater(int c, ScriptList arguments)
        {
            if (c > arguments.Count) throw new ScriptError("Function expects at least " + c + " arguments. It received " +
                arguments.Count + ".");
        }

        public static void ArgumentCountNoMoreThan(int c, ScriptList arguments)
        {
            if (arguments.Count > c) throw new ScriptError("Function expects no more than " + c + " arguments. It received " +
                arguments.Count + ".");
        }

        public ScriptEvaluater()
        {
            this.SetupStandardLibrary();
        }

        public Object EvaluateString(Context context, ScriptObject thisObject, String str, bool discardResults = false)
        {
            var root = Parser.ParseRoot(str);
            return Evaluate(context, root, thisObject, false, discardResults);
        }

        public Object Evaluate(
            Context context,
            ParseNode node,
            ScriptObject thisObject,
            bool ignoreStar = false,
            bool discardResults = false)
        {
            if (context.limitExecutionTime && (DateTime.Now - context.executionStart > context.allowedExecutionTime))
                throw new TimeoutError();

            switch (node.type)
            {
                case NodeType.String:
                    return node.token;
                case NodeType.StringExpression:
                    if (discardResults)
                    {
                        foreach (var piece in node.childNodes)
                        {
                            if (piece.type == NodeType.String)
                                continue;
                            else
                                Evaluate(context, piece, thisObject);
                        }
                        return null;
                    }
                    else
                    {
                        if (node.childNodes.Count == 1)
                            return Evaluate(context, node.childNodes[0], thisObject);
                        else
                        {
                            var resultString = String.Empty;
                            foreach (var piece in node.childNodes)
                                resultString += ScriptObject.AsString(Evaluate(context, piece, thisObject));
                            return resultString;
                        }
                    }
                case NodeType.Token:
                    return LookupToken(context, node.token, thisObject);
                case NodeType.MemberAccess:
                    {
                        var lhs = Evaluate(context, node.childNodes[0], thisObject);
                        String rhs = "";
                        if (node.childNodes[1].type == NodeType.Token)
                            rhs = node.childNodes[1].token;
                        else
                            rhs = ScriptObject.AsString(Evaluate(context, node.childNodes[1], thisObject, false));

                        if (lhs == null) return null;// throw new ScriptError("Left hand side is null.");

                        if (lhs is ScriptObject)
                        {
                            var result = (lhs as ScriptObject).GetProperty(ScriptObject.AsString(rhs));
                            if (result is String && node.token == ":")
                                result = EvaluateString(context, lhs as ScriptObject, result as String);
                            else if (result is ParseNode && node.token == ":")
                                result = Evaluate(context, result as ParseNode, lhs as ScriptObject, true, false);
                            return result;
                        }

                        return null;
                    }
                case NodeType.Node:
                    {
                        if (!ignoreStar && node.prefix == Prefix.Quote)
                            return node;

                        bool eval = node.prefix != Prefix.List;

                        var arguments = new ScriptList();

                        try
                        {
                            foreach (var child in node.childNodes)
                            {
                                bool argumentProcessed = false;
                                bool expectList = false;

                                if (eval && arguments.Count > 0 && (arguments[0] is Function))
                                {
                                    var func = arguments[0] as Function;
                                    int argIndex = arguments.Count - 1;
                                    if (argIndex > func.argumentInfo.Count && func.argumentInfo.Count > 0
                                        && func.argumentInfo[func.argumentInfo.Count - 1].repeat)
                                        argIndex = func.argumentInfo.Count - 1;

                                    if (argIndex < func.argumentInfo.Count)
                                    {
                                        if (func.argumentInfo[argIndex].type == MISP.ArgumentType.CODE)
                                        {
                                            if (child.type == NodeType.Node && child.prefix == Prefix.None)
                                            {
                                                arguments.Add(child);
                                                argumentProcessed = true;
                                            }
                                        }
                                        else if (func.argumentInfo[argIndex].type == MISP.ArgumentType.LIST)
                                            expectList = true;
                                    }
                                }

                                if (!argumentProcessed)
                                {
                                    var argument = Evaluate(context, child, thisObject);
                                    if (argument == null && expectList)  //transform 'null' to an empty list for functions that expect lists
                                        argument = new ScriptList();
                                    if (child.type == NodeType.Node && child.prefix == Prefix.Expand && argument is ScriptList)
                                        arguments.AddRange(argument as ScriptList);
                                    else
                                        arguments.Add(argument);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (arguments.Count > 0 && arguments[0] is Function)
                                throw new ScriptError("[Arg for " + (arguments[0] as Function).name + "] " + e.Message);
                            throw e;
                        }

                        if (node.prefix == Prefix.List) return arguments;


                        Object result = null;
                        if (arguments[0] is Function)
                        {
                            try
                            {
                                result = (arguments[0] as Function).Invoke(context, thisObject,
                                    new ScriptList(arguments.GetRange(1, arguments.Count - 1)));
                            }
                            catch (Exception e)
                            {
                                throw new ScriptError("[" + (arguments[0] as Function).name + "] " + e.Message);
                            }
                        }
                        else
                            result = arguments[0];

                        if (node.prefix == Prefix.Lookup) return LookupToken(context, ScriptObject.AsString(result), thisObject);
                        else return result;
                    }
                case NodeType.Integer:
                    return Convert.ToInt32(node.token);
                case NodeType.DictionaryEntry:
                    {
                        var r = new ScriptList();
                        foreach (var child in node.childNodes)
                            if (child.type == NodeType.Token) r.Add(child.token);
                            else r.Add(Evaluate(context, child, thisObject));
                        return r;
                    }
                default:

                    throw new ScriptError("Internal evaluator error");
            }
        }

        private object LookupToken(Context context, String value, ScriptObject thisObject)
        {
            if (specialVariables.ContainsKey(value)) return specialVariables[value](context, thisObject);
            if (context.HasVariable(value)) return context.GetVariable(value);
            if (functions.ContainsKey(value)) return functions[value];
            if (value.StartsWith("@") && functions.ContainsKey(value.Substring(1))) return functions[value.Substring(1)];
            throw new ScriptError("Could not find value with name " + value + ".");
        }

        public void SetupStandardLibrary()
        {
            #region Built in Specials
            specialVariables.Add("null", (c, s) => { return null; });
            specialVariables.Add("this", (c, s) => { return s; });
            specialVariables.Add("functions", (c, s) => { return new ScriptList(functions.Select((pair) => { return pair.Value; })); });
            specialVariables.Add("true", (c, s) => { return true; });
            #endregion

            #region Foundational Functions

            Func<Context, Object, ScriptList, Object> defunImple = (context, thisObject, arguments) =>
            {
                ArgumentCountOrGreater(4, arguments);
                var functionName = ArgumentType<String>(arguments[0]);

                var argumentInfo = ArgumentInfo.ParseArguments(ArgumentType<ScriptList>(arguments[1]));

                var cVN = ArgumentType<ScriptList>(arguments[2]);
                var closedVariableNames = new List<String>(cVN.Select((o) =>
                {
                    if (!(o is String)) throw new ScriptError("Closed variable names MUST be strings.");
                    return ScriptObject.AsString(o);
                }));

                var functionBody = ArgumentType<ParseNode>(arguments[3]);

                var closedValues = new ScriptList();

                foreach (var closedVariableName in closedVariableNames)
                {
                    if (!context.HasVariable(closedVariableName)) throw new ScriptError("Closed variable not found in parent scope.");
                    closedValues.Add(context.GetVariable(closedVariableName));
                }

                var newFunction = new Function(functionName, argumentInfo, "Script-defined function", (c, to, a) =>
                    {
                        //try
                        //{
                        c.PushScope();
                        for (int i = 0; i < closedValues.Count; ++i)
                            c.PushVariable(closedVariableNames[i], closedValues[i]);
                        for (int i = 0; i < argumentInfo.Count; ++i)
                            c.PushVariable(argumentInfo[i].name, a[i]);

                        var result = Evaluate(c, functionBody, to, true);

                        for (int i = 0; i < argumentInfo.Count; ++i)
                            c.PopVariable(argumentInfo[i].name);
                        for (int i = 0; i < closedValues.Count; ++i)
                            c.PopVariable(closedVariableNames[i]);
                        c.PopScope();

                        return result;
                        //}
                        //catch (ScriptError e)
                        //{
                        //    throw new ScriptError("[defun " + (String.IsNullOrEmpty(functionName) ? "anon" : functionName) + "] " + e.Message);
                        //}
                    });

                if (arguments.Count == 5)
                    newFunction.shortHelp = ScriptObject.AsString(arguments[4]);
                else if (arguments.Count > 5)
                    throw new ScriptError("Too many arguments to defun/lambda.");
                //newFunction.source = sourceSpan(context.activeSource, (arguments[3] as Irony.Parsing.ParseTreeNode).Span);
                newFunction.closedValues = closedValues;
                return newFunction;
            };

            functions.Add("defun", new Function("defun", 
                ArgumentInfo.ParseArguments("string name", "list arguments", "list closures", "code code", "?comment"),
                "name arguments closures code", (context, thisObject, arguments) =>
            {
                var r = defunImple(context, thisObject, arguments);
                if (!String.IsNullOrEmpty((r as Function).name)) functions.Upsert((r as Function).name, r as Function);
                return r;
            }));

            functions.Add("lambda", new Function("lambda",
                ArgumentInfo.ParseArguments("string name", "list arguments", "list closures", "code code", "?comment"),
                    "name arguments closures code : Same as defun with blank name.", (context, thisObject, arguments) =>
            {
                var r = defunImple(context, thisObject, arguments);
                (r as Function).isLambda = true;
                return r;
            }));

            functions.Add("members", new Function("members",
                                   ArgumentInfo.ParseArguments("object object"),
                                   "object : List of names of object members.", (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    return obj.ListProperties();
                }));

            functions.Add("record", new Function("record", 
                ArgumentInfo.ParseArguments("list +?pairs"),
                "<List of key-value pairs> : Returns a new generic script object.",
                (context, thisObject, arguments) =>
                {
                    var r = (new GenericScriptObject()) as ScriptObject;
                    foreach (var item in arguments)
                    {
                        var list = item as ScriptList;
                        if (list == null || list.Count != 2) throw new ScriptError("Record expects only pairs as arguments.");
                        r.SetProperty(ScriptObject.AsString(list[0]), list[1]);
                    }
                    return r;
                }));

            functions.Add("clone", new Function("clone",
                ArgumentInfo.ParseArguments("object record", "list +?pairs"),
                "record <List of key-value pairs> : Returns a new generic script object cloned from [record]",
                (context, thisObject, arguments) =>
                {
                    ArgumentCountOrGreater(1, arguments);
                    var from = ArgumentType<ScriptObject>(arguments[0]);
                    var r = new GenericScriptObject(from);
                    foreach (var item in arguments.GetRange(1, arguments.Count - 1))
                    {
                        var list = item as ScriptList;
                        if (list == null || list.Count != 2) throw new ScriptError("Clone expects only pairs as arguments.");
                        r.SetProperty(ScriptObject.AsString(list[0]), list[1]);
                    }
                    return r;
                }));

            functions.Add("var", new Function("var", 
                ArgumentInfo.ParseArguments("string name", "value"),
                "name value : Assign value to a variable named [name].", (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    if (specialVariables.ContainsKey(ScriptObject.AsString(arguments[0])))
                        throw new ScriptError("Can't assign to protected variable name.");
                    context.ChangeVariable(ScriptObject.AsString(arguments[0]), arguments[1]);
                    return arguments[1];
                }));

            functions.Add("let", new Function("let",
                ArgumentInfo.ParseArguments("list pairs", "code code"),
                "^( ^(\"name\" value) ^(...) ) code : Create temporary variables, run code.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var variables = ArgumentType<ScriptList>(arguments[0]);
                    var code = ArgumentType<ParseNode>(arguments[1]);

                    foreach (var item in variables)
                    {
                        var def = ArgumentType<ScriptList>(item);
                        ArgumentCount(2, def);
                        var name = ArgumentType<String>(def[0]);
                        context.PushVariable(name, def[1]);
                    }

                    var result = Evaluate(context, code, thisObject, true);

                    foreach (var item in variables)
                    {
                        var def = ArgumentType<ScriptList>(item);
                        context.PopVariable(ArgumentType<String>(def[0]));
                    }

                    return result;
                }));

            functions.Add("set", new Function("set", 
                ArgumentInfo.ParseArguments("object object", "string property", "value"),
                "object property value : Set the member of an object.", (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    var vName = ScriptObject.AsString(arguments[1]);
                    obj.SetProperty(vName, arguments[2]);
                    return arguments[2];
                }));

            functions.Add("delete", new Function("delete",
                ArgumentInfo.ParseArguments("object object", "string property"),
                "object property : Deletes a property from an object.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    var vname = ScriptObject.AsString(arguments[1]);
                    var value = obj.GetProperty(vname);
                    obj.DeleteProperty(vname);
                    return value;
                }));



            functions.Add("eval", new Function("eval", 
                ArgumentInfo.ParseArguments("object this", "code code"),
                "thisobject code : Execute code.", (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var _this = ArgumentType<ScriptObject>(arguments[0]);
                    if (arguments[1] is ParseNode)
                        return Evaluate(context, arguments[1] as ParseNode, _this, true);
                    else
                        return EvaluateString(context, _this, ScriptObject.AsString(arguments[1]));
                }));

            functions.Add("lastarg", new Function("lastarg", 
                ArgumentInfo.ParseArguments("+children"),
                "<n> : Returns the last argument.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCountOrGreater(1, arguments);
                    return arguments[arguments.Count - 1];
                }));

            functions.Add("nop", new Function("nop",
                ArgumentInfo.ParseArguments("?+value"),
                "<n> : Returns null.",
                (context, thisObject, arguments) => { return null; }));

            Func<Context, ScriptObject, ScriptList, Object> equalBody =
                (context, thisObject, arguments) =>
                {
                    if (arguments.Count == 0) return null;

                    var nullCount = arguments.Count((o) => { return o == null; });
                    if (nullCount == arguments.Count) return true;
                    if (nullCount > 0) return null;

                    var firstType = arguments[0].GetType();
                    bool allSameType = true;
                    foreach (var argument in arguments)
                        if (argument.GetType() != firstType) allSameType = false;
                    if (!allSameType) return null;
                    for (int i = 1; i < arguments.Count; ++i)
                    {
                        if (firstType == typeof(String))
                        {
                            if (arguments[i] as String != arguments[i - 1] as String) return null;
                        }
                        else if (firstType == typeof(int))
                        {
                            if ((arguments[i] as int?).Value != (arguments[i - 1] as int?).Value) return null;
                        }
                        else if (firstType == typeof(bool))
                        {
                            if ((arguments[i] as bool?).Value != (arguments[i - 1] as bool?).Value) return null;
                        }
                        else if (!Object.ReferenceEquals(arguments[i], arguments[i - 1])) return null;
                    }
                    return true;
                };

            functions.Add("equal", new Function("equal",
                ArgumentInfo.ParseArguments("+value"),
                "<n> : True if all arguments equal, null otherwise.", equalBody));

            functions.Add("notequal", new Function("notequal",
                ArgumentInfo.ParseArguments("+value"),
                "<n> : Null if all arguments equal, true otherwise.",
                (context, thisObject, arguments) =>
                {
                    if (equalBody(context, thisObject, arguments) == null) return true;
                    return null;
                }));

            functions.Add("and", new Function("and", 
                ArgumentInfo.ParseArguments("+value"),
                "<n> : True if all arguments true.",
                (context, thisObject, arguments) =>
                {
                    foreach (var arg in arguments) if (arg == null) return null;
                    return true;
                }));


            functions.Add("atleast", new Function("atleast",
                ArgumentInfo.ParseArguments("integer A", "integer B"),
                "A B : true if A >= B, null otherwise.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    if (first.Value >= second.Value) return true;
                    return null;
                }));

            functions.Add("greaterthan", new Function("greaterthan",
                                ArgumentInfo.ParseArguments("integer A", "integer B"),
"A B : true if A > B, null otherwise.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    if (first.Value > second.Value) return true;
                    return null;
                }));

            functions.Add("nomorethan", new Function("nomorethan",
                                ArgumentInfo.ParseArguments("integer A", "integer B"),
"A B : true if A <= B, null otherwise.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    if (first.Value <= second.Value) return true;
                    return null;
                }));

            functions.Add("lessthan", new Function("lessthan",
                                ArgumentInfo.ParseArguments("integer A", "integer B"),
"A B : true if A < B, null otherwise.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    if (first.Value < second.Value) return true;
                    return null;
                }));

            functions.Add("subtract", new Function("subtract",
                                ArgumentInfo.ParseArguments("integer A", "integer B"),
"A B : return A-B.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    return first.Value - second.Value;
                }));

            functions.Add("add", new Function("add",
                                ArgumentInfo.ParseArguments("integer A", "integer B"),
"A B : return A+B.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    return first.Value + second.Value;
                }));

            functions.Add("random", new Function("random",
                                ArgumentInfo.ParseArguments("integer A", "integer B"),
                "A B : return a random value in range (A,B).",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    return random.Next(first.Value, second.Value);
                }));

            functions.Add("multiply", new Function("multiply",
                                ArgumentInfo.ParseArguments("integer A", "integer B"),
                "A B : return A*B.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    return first.Value * second.Value;
                }));

            functions.Add("not", new Function("not",
                                ArgumentInfo.ParseArguments("value"),
                                "A : true if A is null, null otherwise.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    if (arguments[0] == null) return true;
                    else return null;
                }));

            functions.Add("coalesce", new Function("coalesce",
                ArgumentInfo.ParseArguments("value", "default"),
                "A B : B if A is null, A otherwise.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    if (arguments[0] == null) return arguments[1];
                    return arguments[0];
                }));

            functions.Add("asstring", new Function("asstring",
                ArgumentInfo.ParseArguments("value", "integer B"),
                "A B : convert A to a string to depth B.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var depth = arguments[1] as int?;
                    if (depth == null || !depth.HasValue) return ScriptObject.AsString(arguments[0]);
                    else return ScriptObject.AsString(arguments[0], depth.Value);
                }));
            #endregion

            #region Branching

            functions.Add("if", new Function("if",
                ArgumentInfo.ParseArguments("condition", "code then", "code ?else"),
                "condition then else : If condition evaluates to true, evaluate and return then. Otherwise, evaluate and return else.",
                (context, thisObject, arguments) =>
                {
                    if (arguments.Count != 2 && arguments.Count != 3) throw new ScriptError("If expects two or three arguments");
                    if (arguments[0] != null)
                        return Evaluate(context, arguments[1] as ParseNode, thisObject, true);
                    else if (arguments.Count == 3)
                        return Evaluate(context, arguments[2] as ParseNode, thisObject, true);
                    return null;
                }));

            #endregion

            #region List Manipulation Functions
            
            functions.Add("length", new Function("length", 
                ArgumentInfo.ParseArguments("list"),
                "list : Returns length of list.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    var list = arguments[0] as ScriptList;
                    return list == null ? 0 : list.Count;
                }));

            functions.Add("count", new Function("count", 
                ArgumentInfo.ParseArguments("string variable_name", "list in", "code code"),
                "variable_name list code : Returns number of items in list for which code evaluated to true.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var func = ArgumentType<ParseNode>(arguments[2]);

                    context.PushVariable(vName, null);
                    var result = (int)list.Count((o) =>
                    {
                        context.ChangeVariable(vName, o);
                        return Evaluate(context, func, thisObject, true) != null;
                    });
                    context.PopVariable(vName);
                    return result;
                }));

            functions.Add("cat", new Function("cat",
                ArgumentInfo.ParseArguments("?+items"),
                "<n> : Combine N lists into one",
                (context, thisObject, arguments) =>
                {
                    var result = new ScriptList();
                    foreach (var arg in arguments)
                    {
                        if (arg is ScriptList) result.AddRange(arg as ScriptList);
                        else result.Add(arg);
                    }
                    return result;
                }));

            functions.Add("map", new Function("map",
                ArgumentInfo.ParseArguments("string variable_name", "list in", "code code"), 
                "variable_name list code : Transform one list into another",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var code = ArgumentType<ParseNode>(arguments[2]);
                    var result = new ScriptList();
                    context.PushVariable(vName, null);
                    foreach (var item in list)
                    {
                        context.ChangeVariable(vName, item);
                        result.Add(Evaluate(context, code, thisObject, true));
                    }
                    context.PopVariable(vName);
                    return result;
                }));

            functions.Add("mapi", new Function("mapi",
                ArgumentInfo.ParseArguments("string variable_name", "list in", "code code"),
                "variable_name list code : Like map, except variable_name will hold the index not the value.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var code = ArgumentType<ParseNode>(arguments[2]);
                    var result = new ScriptList();
                    context.PushVariable(vName, null);
                    for (int i = 0; i < list.Count; ++i)
                    {
                        context.ChangeVariable(vName, i);
                        result.Add(Evaluate(context, code, thisObject, true));
                    }
                    context.PopVariable(vName);
                    return result;
                }));

            functions.Add("mapex", new Function("mapex",
                ArgumentInfo.ParseArguments("string variable_name", "start", "code code", "code next"), 
                "variable_name start code next : Like map, but the next element is the result of 'next'. Stops when next = null.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(4, arguments);
                    var vName = ArgumentType<String>(arguments[0]);
                    var code = ArgumentType<ParseNode>(arguments[2]);
                    var next = ArgumentType<ParseNode>(arguments[3]);
                    var result = new ScriptList();
                    var item = arguments[1];
                    context.PushVariable(vName, null);

                    while (item != null)
                    {
                        context.ChangeVariable(vName, item);
                        result.Add(Evaluate(context, code, thisObject, true));
                        item = Evaluate(context, next, thisObject, true);
                    }

                    context.PopVariable(vName);
                    return result;
                }));

            functions.Add("for", new Function("for",
                ArgumentInfo.ParseArguments("string variable_name", "list in", "code code"), 
                "variable_name list code : Execute code for each item in list. Returns result of last run of code.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var func = ArgumentType<ParseNode>(arguments[2]);
                    context.PushVariable(vName, null);
                    Object result = null;
                    foreach (var item in list)
                    {
                        context.ChangeVariable(vName, item);
                        result = Evaluate(context, func, thisObject, true);
                    }

                    context.PopVariable(vName);

                    return result;
                }));

            functions.Add("where", new Function("where",
                ArgumentInfo.ParseArguments("string variable_name", "list in", "code code"),
                "variable_name list code : Returns new list containing only the items in list for which code evaluated to true.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var func = ArgumentType<ParseNode>(arguments[2]);

                    context.PushVariable(vName, null);
                    var result = new ScriptList(list.Where((o) =>
                        {
                            context.ChangeVariable(vName, o);
                            return Evaluate(context, func, thisObject, true) != null;
                        }));
                    context.PopVariable(vName);
                    return result;
                }));

            functions.Add("while", new Function("while",
                ArgumentInfo.ParseArguments("code condition", "code code"),
                "condition code : Repeat code while condition evaluates to true.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var cond = ArgumentType<ParseNode>(arguments[0]);
                    var code = ArgumentType<ParseNode>(arguments[1]);

                    while (Evaluate(context, cond, thisObject, true) != null)
                        Evaluate(context, code, thisObject, true);
                    return null;
                }));

            functions.Add("last", new Function("last",
                ArgumentInfo.ParseArguments("list list"),
                "list : Returns last item in list.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    if (list.Count == 0) return null;
                    return list[list.Count - 1];
                }));

            functions.Add("first", new Function("first",
                ArgumentInfo.ParseArguments("list list"),
                "list : Returns first item in list.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    if (list.Count == 0) return null;
                    return list[0];
                }));

            functions.Add("index", new Function("index",
                ArgumentInfo.ParseArguments("list list", "integer n"),
                "list n : Returns nth element in list.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    var index = arguments[1] as int?;
                    if (index == null || !index.HasValue) return null;
                    if (index.Value < 0 || index.Value >= list.Count) return null;
                    return list[index.Value];
                }));

            functions.Add("sub-list", new Function("sub-list",
                ArgumentInfo.ParseArguments("list list", "integer start", "integer ?length"), "list start length: Returns a elements in list between start and start+length.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCountOrGreater(2, arguments);
                    ArgumentCountNoMoreThan(3, arguments);
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    var start = arguments[1] as int?;
                    if (start == null || !start.HasValue) return new ScriptList();
                    int? length = null;
                    if (arguments.Count == 3) length = arguments[2] as int?;
                    else length = list.Count;
                    if (length == null || !length.HasValue) return new ScriptList();

                    if (start.Value < 0) { length -= start; start = 0; }
                    if (start.Value >= list.Count) return new ScriptList();
                    if (length.Value <= 0) return new ScriptList();
                    if (length.Value + start.Value >= list.Count) length = list.Count - start.Value;

                    return new ScriptList(list.GetRange(start.Value, length.Value));
                }));

            functions.Add("sort", new Function("sort",
                ArgumentInfo.ParseArguments("string variable_name", "list in", "code code"), 
                "vname list sort_func: Sorts elements according to sort func; sort func returns integer used to order items.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var vName = ScriptObject.AsString(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var sortFunc = ArgumentType<ParseNode>(arguments[2]);

                    var comparer = new ListSortComparer(this, vName, sortFunc, context, thisObject);
                    list.Sort(comparer);
                    return list;
                }));

            functions.Add("reverse", new Function("reverse",
                ArgumentInfo.ParseArguments("list list"),
                "list: Reverse the list.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    list.Reverse();
                    return list;
                }));

            #endregion

            #region String Functions
            functions.Add("substr", new Function("substr",
                ArgumentInfo.ParseArguments("value", "integer start"),
                "string start : returns sub-string of string starting at start.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var index = (arguments[1] as int?).Value;
                    var str = ScriptObject.AsString(arguments[0]);
                    if (index >= str.Length) return "";
                    if (index < 0) return str;
                    return str.Substring(index);
                }));

            functions.Add("strcat", new Function("strcat",
                ArgumentInfo.ParseArguments("?+item"), "<n> : Concatenate many strings into one.",
                (context, thisObject, arguments) =>
                {
                    var r = "";
                    foreach (var obj in arguments) if (obj == null) r += "null"; else r += ScriptObject.AsString(obj);
                    return r;
                }));

            functions.Add("strrepeat", new Function("strrepeat",
                ArgumentInfo.ParseArguments("integer n", "string part"),
                "n part: Create a string consisting of part n times.",
                    (context, thisObject, arguments) =>
                    {
                        ArgumentCount(2, arguments);
                        var count = arguments[0] as int?;
                        if (count == null | !count.HasValue) throw new ScriptError("Expected int");
                        var part = ScriptObject.AsString(arguments[1]);
                        var r = "";
                        for (int i = 0; i < count.Value; ++i) r += part;
                        return r;
                    }
            ));
            #endregion
        }

        private class ListSortComparer : IComparer<Object>
        {
            ScriptEvaluater evaluater;
            ScriptObject thisObject;
            Context context;
            ParseNode func;
            String vName;

            internal ListSortComparer(ScriptEvaluater evaluater,
                String vName, ParseNode func, Context context, ScriptObject thisObject)
            {
                this.evaluater = evaluater;
                this.vName = vName;
                this.func = func;
                this.context = context;
                this.thisObject = thisObject;
            }

            private int rank(Object o)
            {
                context.PushVariable(vName, o);
                var r = evaluater.Evaluate(context, func, thisObject, true) as int?;
                context.PopVariable(vName);
                if (r != null && r.HasValue) return r.Value;
                return 0;
            }

            public int Compare(object x, object y)
            {
                return rank(x) - rank(y);
            }
        }
    }
}
