using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class ScriptError : Exception { public ScriptError(String msg) : base(msg) { } }
    public class TimeoutError : ScriptError
    {
        public TimeoutError() : base("Execution timed out.") { }
    }

    public class ScriptEvaluater
    {
        public Dictionary<String, ScriptFunction> functions = new Dictionary<String, ScriptFunction>();
        public Dictionary<String, Func<ScriptContext, ScriptObject, Object>> specialVariables
            = new Dictionary<string, Func<ScriptContext, ScriptObject, object>>();
        public MudCore core { get; private set; }
        
        public TimeSpan allowedExecutionTime = TimeSpan.FromSeconds(10);

        public static T ArgumentType<T>(Object obj) where T : class
        {
            var r = obj as T;
            if (r == null) 
                throw new ScriptError("Function argument is the wrong type");
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

        public ScriptEvaluater(MudCore core)
        {
            this.core = core;
        }

        public Object EvaluateString(ScriptContext context, ScriptObject thisObject, String str, bool discardResults = false)
        {
            context.activeSource = str;
            var root = ScriptParser.ParseRoot(str);
            return Evaluate(context, root, thisObject, false, discardResults);
        }

        public Object Evaluate(
            ScriptContext context,
            ParseNode node,
            ScriptObject thisObject,
            bool ignoreStar = false,
            bool discardResults = false)
        {
            //if (DateTime.Now - context.executionStart > allowedExecutionTime) throw new TimeoutError();

            if (node.type == "string")
            {
                return node.token;
            }
            else if (node.type == "string expression")
            {
                if (discardResults)
                {
                    foreach (var piece in node.childNodes)
                    {
                        if (piece.type == "string")
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
            }
            else if (node.type == "token")
            {
                return LookupToken(context, node.token, thisObject);
            }
            else if (node.type == "member access")
            {
                var lhs = Evaluate(context, node.childNodes[0], thisObject);
                String rhs = "";
                if (node.childNodes[1].type == "token")
                    rhs = node.childNodes[1].token;
                else
                    rhs = ScriptObject.AsString(Evaluate(context, node.childNodes[1], thisObject, false));

                if (lhs == null) return null;// throw new ScriptError("Left hand side is null.");

                if (lhs is ScriptObject)
                {
                    var result = (lhs as ScriptObject).GetProperty(ScriptObject.AsString(rhs));
                    if (result is String && node.token == ":")
                        result = EvaluateString(context, lhs as ScriptObject, result as String);
                    else if (result is ParseNode && node.childNodes[1].token == ":")
                        result = Evaluate(context, result as ParseNode, lhs as ScriptObject, true, false);
                    return result;
                }

                return null;
                //throw new ScriptError("Left hand side [" + ScriptObject.AsString(lhs) + "] is not a script object.");
            }
            else if (node.type == "node")
            {
                if (!ignoreStar && node.token == "*")
                    return node;

                var arguments = new ScriptList();
                try
                {
                    foreach (var child in node.childNodes)
                    {
                        var argument = Evaluate(context, child, thisObject);
                        if (child.type == "node" && child.token == "$" && argument is ScriptList)
                            arguments.AddRange(argument as ScriptList);
                        else
                            arguments.Add(argument);
                    }
                }
                catch (Exception e)
                {
                    if (arguments.Count > 0 && arguments[0] is ScriptFunction)
                        throw new ScriptError("[Arg for " + (arguments[0] as ScriptFunction).name + "] " + e.Message);
                    throw e;
                }

                if (node.token == "^") return arguments;

                if (node.token != "*" || ignoreStar)
                {
                    Object result = null;
                    if (arguments[0] is ScriptFunction)
                    {
                        try
                        {
                            result = (arguments[0] as ScriptFunction).Invoke(context, thisObject,
                                new ScriptList(arguments.GetRange(1, arguments.Count - 1)));
                        }
                        catch (Exception e)
                        {
                            throw new ScriptError("[" + (arguments[0] as ScriptFunction).name + "] " + e.Message);
                        }
                    }
                    else
                        result = arguments[0];

                    if (node.token == "#") return LookupToken(context, ScriptObject.AsString(result), thisObject);
                    else return result;
                }
            }
            else if (node.type == "integer")
            {
                return Convert.ToInt32(node.token);
            }
            
            throw new ScriptError("Internal evaluator error");
        }

        private object LookupToken(ScriptContext context, String value, ScriptObject thisObject)
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

            Func<ScriptContext, Object, ScriptList, Object> defunImple = (context, thisObject, arguments) =>
            {
                ArgumentCountOrGreater(4, arguments);
                var functionName = ArgumentType<String>(arguments[0]);

                var aN = ArgumentType<ScriptList>(arguments[1]);
                var argumentNames = new List<String>(aN.Select((o) =>
                {
                    if (!(o is String)) throw new ScriptError("Variable names MUST be strings.");
                    return ScriptObject.AsString(o);
                }));

                var cVN = ArgumentType<ScriptList>(arguments[2]);
                var closedVariableNames = new List<String>(cVN.Select((o) =>
                {
                    if (!(o is String)) throw new ScriptError("Closed variable names MUST be strings.");
                    return ScriptObject.AsString(o);
                }));

                var functionBody = ArgumentType<ParseNode>(arguments[3]);

                var closedValues = new ScriptList();

                foreach (var closedVariableName in closedVariableNames)
                    closedValues.Add(context.GetVariable(closedVariableName));

                var newFunction = new ScriptFunction(functionName, "Script-defined function", (c, to, a) =>
                    {
                        //try
                        //{
                        c.PushScope();
                        for (int i = 0; i < closedValues.Count; ++i)
                            c.PushVariable(closedVariableNames[i], closedValues[i]);
                        for (int i = 0; i < argumentNames.Count; ++i)
                            c.PushVariable(argumentNames[i], a[i]);

                        var result = Evaluate(c, functionBody, to, true);

                        for (int i = 0; i < argumentNames.Count; ++i)
                            c.PopVariable(argumentNames[i]);
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
            
            functions.Add("defun", new ScriptFunction("defun", "name arguments closures code", (context, thisObject, arguments) =>
            {
                var r = defunImple(context, thisObject, arguments);
                if (!String.IsNullOrEmpty((r as ScriptFunction).name)) functions.Upsert((r as ScriptFunction).name, r as ScriptFunction);
                return r;
            }));

            functions.Add("lambda", new ScriptFunction("lambda", "name arguments closures code : Same as defun with blank name.", (context, thisObject, arguments) =>
            {
                var r = defunImple(context, thisObject, arguments);
                (r as ScriptFunction).isLambda = true;
                return r;
            }));

            functions.Add("members", new ScriptFunction("members", "object : List of names of object members.", (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    return obj.ListProperties();
                }));

            functions.Add("record", new ScriptFunction("record", "<List of key-value pairs> : Returns a new generic script object.",
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

            functions.Add("clone", new ScriptFunction("clone", "record <List of key-value pairs> : Returns a new generic script object cloned from [record]",
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

            functions.Add("var", new ScriptFunction("var", "name value : Assign value to a variable named [name].", (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    if (specialVariables.ContainsKey(ScriptObject.AsString(arguments[0])))
                        throw new ScriptError("Can't assign to protected variable name.");
                    context.ChangeVariable(ScriptObject.AsString(arguments[0]), arguments[1]);
                    return arguments[1];
                }));

            functions.Add("let", new ScriptFunction("let", "^( ^(\"name\" value) ^(...) ) code : Create temporary variables, run code.",
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

            functions.Add("set", new ScriptFunction("set", "object property value : Set the member of an object.", (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    var vName = ScriptObject.AsString(arguments[1]);
                    obj.SetProperty(vName, arguments[2]);
                    return arguments[2];
                }));

            functions.Add("delete", new ScriptFunction("delete", "object property : Deletes a property from an object.", 
                (context, thisObject, arguments) =>
                    {
                        ArgumentCount(2, arguments);
                        var obj = ArgumentType<ScriptObject>(arguments[0]);
                        var vname = ScriptObject.AsString(arguments[1]);
                        var value = obj.GetProperty(vname);
                        obj.DeleteProperty(vname);
                        return value;
                    }));     
            
           

            functions.Add("eval", new ScriptFunction("eval", "code : Execute code.", (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    return EvaluateString(context, thisObject, ScriptObject.AsString(arguments[0]));
                }));

            functions.Add("lastarg", new ScriptFunction("lastarg", "<n> : Returns the last argument.", 
                (context, thisObject, arguments) =>
                {
                    ArgumentCountOrGreater(1, arguments);
                    return arguments[arguments.Count - 1];
                }));

            functions.Add("nop", new ScriptFunction("nop", "<n> : Returns null.",
                (context, thisObject, arguments) => { return null; }));

            Func<ScriptContext, ScriptObject, ScriptList, Object> equalBody =
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

            functions.Add("equal", new ScriptFunction("equal", "<n> : True if all arguments equal, null otherwise.", equalBody));

            functions.Add("notequal", new ScriptFunction("notequal", "<n> : Null if all arguments equal, true otherwise.",
                (context, thisObject, arguments) =>
                {
                    if (equalBody(context, thisObject, arguments) == null) return true;
                    return null;
                }));

            functions.Add("and", new ScriptFunction("and", "<n> : True if all arguments true.",
                (context, thisObject, arguments) =>
                {
                    foreach (var arg in arguments) if (arg == null) return null;
                    return true;
                }));


            functions.Add("atleast", new ScriptFunction("atleast", "A B : true if A >= B, null otherwise.", 
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    if (first.Value >= second.Value) return true;
                    return null;
                }));

            functions.Add("greaterthan", new ScriptFunction("greaterthan", "A B : true if A > B, null otherwise.", 
                (context, thisObject, arguments) =>
            {
                ArgumentCount(2, arguments);
                var first = arguments[0] as int?;
                var second = arguments[1] as int?;
                if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                if (first.Value > second.Value) return true;
                return null;
            }));

            functions.Add("nomorethan", new ScriptFunction("nomorethan", "A B : true if A <= B, null otherwise.", 
                (context, thisObject, arguments) =>
            {
                ArgumentCount(2, arguments);
                var first = arguments[0] as int?;
                var second = arguments[1] as int?;
                if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                if (first.Value <= second.Value) return true;
                return null;
            }));

            functions.Add("lessthan", new ScriptFunction("lessthan", "A B : true if A < B, null otherwise.", 
                (context, thisObject, arguments) =>
            {
                ArgumentCount(2, arguments);
                var first = arguments[0] as int?;
                var second = arguments[1] as int?;
                if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                if (first.Value < second.Value) return true;
                return null;
            }));

            functions.Add("minus", new ScriptFunction("minus", "A B : return A-B.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    return first.Value - second.Value;
                }));

            functions.Add("not", new ScriptFunction("not", "A : true if A is null, null otherwise.", 
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    if (arguments[0] == null) return true;
                    else return null;
                }));

            functions.Add("coalesce", new ScriptFunction("coalesce", "A B : B if A is null, A otherwise.", 
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    if (arguments[0] == null) return arguments[1];
                    return arguments[0];
                }));

            functions.Add("asstring", new ScriptFunction("asstring", "A B : convert A to a string to depth B.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var depth = arguments[1] as int?;
                    if (depth == null || !depth.HasValue) return ScriptObject.AsString(arguments[0]);
                    else return ScriptObject.AsString(arguments[0], depth.Value);
                }));
            #endregion

            #region Branching

            functions.Add("if", new ScriptFunction("if",
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
            functions.Add("list", new ScriptFunction("list", "<n> : Returns arguments as list.",
                (context, thisObject, arguments) =>
            {
                return arguments;
            }));

            functions.Add("length", new ScriptFunction("length", "list : Returns length of list.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    var list = arguments[0] as ScriptList;
                    return list == null ? 0 : list.Count;
                }));

            functions.Add("count", new ScriptFunction("count", "variable_name list code : Returns number of items in list for which code evaluated to true.",
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

            functions.Add("cat", new ScriptFunction("cat", "<n> : Combine N lists into one",
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

            functions.Add("map", new ScriptFunction("map", "variable_name list code : Transform one list into another", 
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

            functions.Add("mapi", new ScriptFunction("mapi", "variable_name list code : Like map, except variable_name will hold the index not the value.",
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

            functions.Add("mapex", new ScriptFunction("mapex", "variable_name start code next : Like map, but the next element is the result of 'next'. Stops when next = null.",
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
            
            functions.Add("for", new ScriptFunction("for", "variable_name list code : Execute code for each item in list. Returns result of last run of code.", 
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

            functions.Add("where", new ScriptFunction("where", 
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

            functions.Add("while", new ScriptFunction("while",
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

            functions.Add("last", new ScriptFunction("last", "list : Returns last item in list.", 
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    if (list.Count == 0) return null;
                    return list[list.Count - 1];
                }));

            functions.Add("first", new ScriptFunction("first", "list : Returns first item in list.", 
                (context, thisObject, arguments) =>
            {
                ArgumentCount(1, arguments);
                var list = ArgumentType<ScriptList>(arguments[0]);
                if (list.Count == 0) return null;
                return list[0];
            }));

            functions.Add("index", new ScriptFunction("index", "list n : Returns nth element in list.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    var index = arguments[1] as int?;
                    if (index == null || !index.HasValue) return null;
                    if (index.Value < 0 || index.Value >= list.Count) return null;
                    return list[index.Value];
                }));
            #endregion

            #region String Functions
            functions.Add("substr", new ScriptFunction("substr", "string start : returns sub-string of string starting at start.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var index = (arguments[1] as int?).Value;
                    var str = ScriptObject.AsString(arguments[0]);
                    if (index >= str.Length) return "";
                    if (index < 0) return str;
                    return str.Substring(index);
                }));

            functions.Add("strcat", new ScriptFunction("strcat", "<n> : Concatenate many strings into one.", 
                (context, thisObject, arguments) =>
                {
                    var r = "";
                    foreach (var obj in arguments) if (obj == null) r += "null"; else r += ScriptObject.AsString(obj);
                    return r;
                }));
            #endregion
        }
    }
}
