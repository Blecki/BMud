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
        private Irony.Parsing.Parser parser = new Irony.Parsing.Parser(new ScriptGrammar());
        public Dictionary<String, Func<ScriptContext, ScriptObject, Object>> specialVariables
            = new Dictionary<string, Func<ScriptContext, ScriptObject, object>>();
        public MudCore core { get; private set; }
        
        public TimeSpan allowedExecutionTime = TimeSpan.FromSeconds(10);

        public static String sourceSpan(String source, Irony.Parsing.SourceSpan span)
        {
            return source.Substring(span.Location.Position, span.Length);
        }

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
            var root = parser.Parse(str);
            if (root.HasErrors())
                throw new ScriptError(root.ParserMessages[0].Message);
            return Evaluate(context, root.Root, thisObject, false, discardResults);
        }

        public Object Evaluate(
            ScriptContext context,
            Irony.Parsing.ParseTreeNode node,
            ScriptObject thisObject,
            bool ignoreStar = false,
            bool discardResults = false)
        {
            //if (DateTime.Now - context.executionStart > allowedExecutionTime) throw new TimeoutError();

            if (node.Term.Name == "Root")
            {
                if (discardResults)
                {
                    foreach (var piece in node.ChildNodes)
                    {
                        if (piece.Term.Name == "Text Literal")
                            continue;
                        Evaluate(context, piece, thisObject);
                    }
                    return null;
                }
                else
                {
                    if (node.ChildNodes.Count == 1)
                    {
                        if (node.ChildNodes[0].Term.Name == "Text Literal")
                            return node.ChildNodes[0].FindTokenAndGetText();
                        else
                            return Evaluate(context, node.ChildNodes[0], thisObject);
                    }
                    else
                    {
                        var resultString = String.Empty;
                        foreach (var piece in node.ChildNodes)
                            if (piece.Term.Name == "Text Literal")
                                resultString += piece.FindTokenAndGetText();
                            else
                                resultString += ScriptObject.AsString(Evaluate(context, piece, thisObject));
                        return resultString;
                    }
                }
            }
            else if (node.Term.Name == "Token")
            {
                var value = node.FindTokenAndGetText();
                if (specialVariables.ContainsKey(value)) return specialVariables[value](context, thisObject);
                if (context.HasVariable(value)) return context.GetVariable(value);
                if (functions.ContainsKey(value)) return functions[value];
                throw new ScriptError("Could not find value with name " + value + ".");
            }
            else if (node.Term.Name == "Embedded String")
            {
                if (node.ChildNodes[0].ChildNodes.Count > 0) return node.ChildNodes[2];
                var r = Evaluate(context, node.ChildNodes[2], thisObject);
                return ScriptObject.AsString(r);
            }
            else if (node.Term.Name == "Member Access")
            {
                var lhs = Evaluate(context, node.ChildNodes[0], thisObject);
                String rhs = "";
                if (node.ChildNodes[2].FirstChild.Term.Name == "Token")
                    rhs = node.ChildNodes[2].FirstChild.FindTokenAndGetText();
                else
                    rhs = ScriptObject.AsString(Evaluate(context, node.ChildNodes[2].FirstChild, thisObject, false));
                //var rhs = node.ChildNodes[2].FindTokenAndGetText();// Evaluate(context, node.ChildNodes[2], thisObject);

                if (lhs == null) throw new ScriptError("Left hand side is null.");

                if (lhs is ScriptObject)
                {
                    var result = (lhs as ScriptObject).GetProperty(ScriptObject.AsString(rhs));
                    if (result is String && node.ChildNodes[1].FindTokenAndGetText() == ":")
                        result = EvaluateString(context, lhs as MudObject, result as String);
                    else if (result is Irony.Parsing.ParseTreeNode && node.ChildNodes[1].FindTokenAndGetText() == ":")
                        result = Evaluate(context, result as Irony.Parsing.ParseTreeNode, lhs as MudObject, true, false);
                    return result;
                }

                throw new ScriptError("Left hand side [" + ScriptObject.AsString(lhs) + "] is not a script object.");
            }
            else if (node.Term.Name == "Node")
            {
                
                    if (!ignoreStar && node.ChildNodes[0].ChildNodes.Count > 0 && node.ChildNodes[0].FindTokenAndGetText() == "*")
                        return node;

                    var arguments = new ScriptList();
                    try
                    {
                        foreach (var child in node.ChildNodes[2].ChildNodes)
                        {
                            var argument = Evaluate(context, child, thisObject);
                            if (child.Term.Name == "Node" && child.ChildNodes[0].FindTokenAndGetText() == "$" && argument is ScriptList)
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

                    //Call function referenced by first argument
                    if (node.ChildNodes[0].ChildNodes.Count == 0 || node.ChildNodes[0].FindTokenAndGetText() != "^")
                    {
                        if (arguments[0] is ScriptFunction)
                        {
                            try
                            {
                                return (arguments[0] as ScriptFunction).Invoke(context, thisObject,
                                    new ScriptList(arguments.GetRange(1, arguments.Count - 1)));
                            }
                            catch (Exception e)
                            {
                                throw new ScriptError("[" + (arguments[0] as ScriptFunction).name + "] " + e.Message);
                            }
                        }
                        else
                            return arguments[0];
                        //throw new ScriptError("First item is not a function.");
                    }
                    else
                        return arguments;
                
            }
            else if (node.Term.Name == "Integer")
            {
                return Convert.ToInt32(node.FindTokenAndGetText());
            }
            
            throw new ScriptError("Internal evaluator error");
        }

        public void SetupStandardLibrary()
        { 
            #region Built in Specials
            specialVariables.Add("null", (c, s) => { return null; });
            specialVariables.Add("this", (c, s) => { return s; });
            specialVariables.Add("functions", (c, s) => { return new ScriptList(functions.Select((pair) => { return pair.Value; })); });
            #endregion

            #region Foundational Functions

            functions.Add("defun", new ScriptFunction("defun", "name arguments closures code", (context, thisObject, arguments) =>
            {
                ArgumentCount(4, arguments);
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

                var functionBody = ArgumentType<Irony.Parsing.ParseTreeNode>(arguments[3]);

                var closedValues = new ScriptList();

                foreach (var closedVariableName in closedVariableNames)
                    closedValues.Add(context.GetVariable(closedVariableName));

                var newFunction = new ScriptFunction(functionName, "Script-defined function", (c, to, a) =>
                    {
                        //try
                        //{
                            for (int i = 0; i < closedValues.Count; ++i)
                                c.PushVariable(closedVariableNames[i], closedValues[i]);
                            for (int i = 0; i < argumentNames.Count; ++i)
                                c.PushVariable(argumentNames[i], a[i]);

                            var result = Evaluate(c, functionBody, to, true);

                            for (int i = 0; i < argumentNames.Count; ++i)
                                c.PopVariable(argumentNames[i]);
                            for (int i = 0; i < closedValues.Count; ++i)
                                c.PopVariable(closedVariableNames[i]);

                            return result;
                        //}
                        //catch (ScriptError e)
                        //{
                        //    throw new ScriptError("[defun " + (String.IsNullOrEmpty(functionName) ? "anon" : functionName) + "] " + e.Message);
                        //}
                    });

                //newFunction.source = sourceSpan(context.activeSource, (arguments[3] as Irony.Parsing.ParseTreeNode).Span);

                if (!String.IsNullOrEmpty(functionName)) functions.Upsert(functionName, newFunction);
                return newFunction;
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
                    context.PushVariable(ScriptObject.AsString(arguments[0]), arguments[1]);
                    return arguments[1];
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

            functions.Add("equal", new ScriptFunction("equal", "<n> : True if all arguments equal, null otherwise.",
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
                        else if (!Object.ReferenceEquals(arguments[i], arguments[i - 1])) return null;
                    }
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
            #endregion

            #region Branching

            functions.Add("if", new ScriptFunction("if",
                "condition then else : If condition evaluates to true, evaluate and return then. Otherwise, evaluate and return else.", 
                (context, thisObject, arguments) =>
                {
                    if (arguments.Count != 2 && arguments.Count != 3) throw new ScriptError("If expects two or three arguments");
                    if (arguments[0] != null) 
                        return Evaluate(context, arguments[1] as Irony.Parsing.ParseTreeNode, thisObject, true);
                    else if (arguments.Count == 3)
                        return Evaluate(context, arguments[2] as Irony.Parsing.ParseTreeNode, thisObject, true);
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
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    return list.Count;
                }));

            functions.Add("count", new ScriptFunction("count", "variable_name list code : Returns number of items in list for which code evaluated to true.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var func = ArgumentType<Irony.Parsing.ParseTreeNode>(arguments[2]);

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
                    var code = ArgumentType<Irony.Parsing.ParseTreeNode>(arguments[2]);
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


            functions.Add("for", new ScriptFunction("for", "variable_name list code : Execute code for each item in list", 
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = arguments[1] as ScriptList;
                    var func = arguments[2] as Irony.Parsing.ParseTreeNode;
                    context.PushVariable(vName, null);

                    foreach (var item in list)
                    {
                        context.ChangeVariable(vName, item);
                        Evaluate(context, func, thisObject, true);
                    }

                    context.PopVariable(vName);

                    return null;
                }));

            functions.Add("where", new ScriptFunction("where", 
                "variable_name list code : Returns new list containing only the items in list for which code evaluated to true.", 
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var func = ArgumentType<Irony.Parsing.ParseTreeNode>(arguments[2]);

                    context.PushVariable(vName, null);
                    var result = new ScriptList(list.Where((o) =>
                        {
                            context.ChangeVariable(vName, o);
                            return Evaluate(context, func, thisObject, true) != null;
                        }));
                    context.PopVariable(vName);
                    return result;
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
