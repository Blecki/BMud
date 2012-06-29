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
        //public Dictionary<String, Func<ScriptContext, MudObject, List<Object>, Object>> functions = 
        //    new Dictionary<String, Func<ScriptContext, MudObject, List<Object>, Object>>();
        private Irony.Parsing.Parser parser = new Irony.Parsing.Parser(new ScriptGrammar());
        public MudCore core { get; private set; }

        public TimeSpan allowedExecutionTime = TimeSpan.FromSeconds(10);

        public static T ArgumentType<T>(Object obj) where T : class
        {
            var r = obj as T;
            if (r == null) 
                throw new ScriptError("Function argument is the wrong type");
            return r;
        }

        public static void ArgumentCount(int c, List<Object> arguments)
        {
            if (c != arguments.Count) throw new ScriptError("Function expects " + c + " arguments. It received " +
                arguments.Count + ".");
        }

        public static void ArgumentCountOrGreater(int c, List<Object> arguments)
        {
            if (c > arguments.Count) throw new ScriptError("Function expects at least " + c + " arguments. It received " +
                arguments.Count + ".");
        }

        public ScriptEvaluater(MudCore core)
        {
            this.core = core;
        }

        public Object EvaluateString(ScriptContext context, ScriptObject thisObject, String str)
        {
            
            var root = parser.Parse(str);
            if (root.HasErrors())
                throw new ScriptError(root.ParserMessages[0].Message);
            return Evaluate(context, root.Root, thisObject);
        }

        public Object Evaluate(
            ScriptContext context,
            Irony.Parsing.ParseTreeNode node,
            ScriptObject thisObject,
            bool ignoreStar = false)
        {
            //if (DateTime.Now - context.executionStart > allowedExecutionTime) throw new TimeoutError();

            if (node.Term.Name == "Root")
            {
                bool resultIsString = false;
                String resultString = String.Empty;
                Object resultObject = null;
                foreach (var piece in node.ChildNodes)
                {
                    if (piece.Term.Name == "Text Literal")
                    {
                        if (resultObject != null)
                        {
                            resultString += resultObject.ToString();
                            resultObject = null;
                        }
                        resultIsString = true;
                        resultString += piece.FindTokenAndGetText();
                    }
                    else
                    {
                        var resultPiece = Evaluate(context, piece, thisObject);
                        if (resultPiece != null)
                        {
                            if (resultObject != null)
                            {
                                resultIsString = true;
                                resultString += resultObject.ToString();
                            }
                            if (resultIsString) resultString += resultPiece.ToString();
                            else resultObject = resultPiece;
                        }
                    }
                }
                if (resultIsString) return resultString;
                return resultObject == null ? "" : resultObject;
            }
            if (node.Term.Name == "Token")
            {
                var value = node.FindTokenAndGetText();
                if (value == "null") return null;
                if (value == "this") return thisObject;
                if (value == "functions") return new List<Object>(functions.Select((pair) => { return pair.Value; }));
                if (context.HasVariable(value)) return context.GetVariable(value);
                if (functions.ContainsKey(value)) return functions[value];
                throw new ScriptError("Could not find value with name " + value + ".");
            }
            else if (node.Term.Name == "Embedded String")
            {
                if (node.ChildNodes[0].ChildNodes.Count > 0) return node.ChildNodes[2];
                var r = Evaluate(context, node.ChildNodes[2], thisObject);
                return r.ToString();
            }
            else if (node.Term.Name == "Member Access")
            {
                var lhs = Evaluate(context, node.ChildNodes[0], thisObject);
                var rhs = node.ChildNodes[2].FindTokenAndGetText();// Evaluate(context, node.ChildNodes[2], thisObject);

                if (lhs == null) throw new ScriptError("Left hand side is null.");

                if (lhs is ScriptObject)
                {
                    var result = (lhs as ScriptObject).GetProperty(rhs.ToString());
                    if (result is String && node.ChildNodes[1].FindTokenAndGetText() == ":")
                        result = EvaluateString(context, lhs as MudObject, result as String);
                    return result;
                }

                throw new ScriptError("Left hand side is not a script object.");
            }
            else if (node.Term.Name == "Node")
            {
                
                    if (!ignoreStar && node.ChildNodes[0].ChildNodes.Count > 0 && node.ChildNodes[0].FindTokenAndGetText() == "*")
                        return node;

                    var arguments = new List<Object>();
                    foreach (var child in node.ChildNodes[2].ChildNodes)
                    {
                        var argument = Evaluate(context, child, thisObject);
                        if (child.Term.Name == "Node" && child.ChildNodes[0].FindTokenAndGetText() == "$" && argument is List<Object>)
                            arguments.AddRange(argument as List<Object>);
                        else
                            arguments.Add(argument);
                    }

                    //Call function referenced by first argument
                    if (node.ChildNodes[0].ChildNodes.Count == 0 || node.ChildNodes[0].FindTokenAndGetText() != "^")
                    {
                        if (arguments[0] is ScriptFunction)
                        {
                            try
                            {
                                return (arguments[0] as ScriptFunction).Invoke(context, thisObject,
                                    new List<Object>(arguments.GetRange(1, arguments.Count - 1)));
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
                return Convert.ToInt64(node.FindTokenAndGetText());
            }
            else if (node.Term.Name == "Object Literal")
            {
                return core.database.LoadObject(node.FindTokenAndGetText().Substring(1), context.actor);
            }
            throw new ScriptError("Internal evaluator error");
        }

        public void SetupStandardLibrary()
        {
            #region Foundational Functions
            functions.Add("defun", new ScriptFunction("defun", (context, thisObject, arguments) =>
            {
                ArgumentCount(4, arguments);
                var functionName = arguments[0].ToString();
                var argumentNames = ArgumentType<List<Object>>(arguments[1]);
                var closedVariableNames = ArgumentType<List<Object>>(arguments[2]);
                var functionBody = ArgumentType<Irony.Parsing.ParseTreeNode>(arguments[3]);

                var closedValues = new List<Object>();

                foreach (var closedVariableName in closedVariableNames)
                    closedValues.Add(context.GetVariable(closedVariableName.ToString()));

                var newFunction = new ScriptFunction(functionName, (c, to, a) =>
                    {
                        //try
                        //{
                            for (int i = 0; i < closedValues.Count; ++i)
                                c.PushVariable(closedVariableNames[i].ToString(), closedValues[i]);
                            for (int i = 0; i < (arguments[1] as List<Object>).Count; ++i)
                                c.PushVariable((arguments[1] as List<Object>)[i].ToString(), a[i]);

                            var result = Evaluate(c, arguments[3] as Irony.Parsing.ParseTreeNode, to, true);

                            for (int i = 0; i < (arguments[1] as List<Object>).Count; ++i)
                                c.PopVariable((arguments[1] as List<Object>)[i].ToString());
                            for (int i = 0; i < closedValues.Count; ++i)
                                c.PopVariable(closedVariableNames[i].ToString());

                            return result;
                        //}
                        //catch (ScriptError e)
                        //{
                        //    throw new ScriptError("[defun " + functionName + "] " + e.Message);
                        //}
                    });

                if (!String.IsNullOrEmpty(functionName)) functions.Add(functionName, newFunction);
                return newFunction;
            }));

            functions.Add("var", new ScriptFunction("var", (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    context.PushVariable(arguments[0].ToString(), arguments[1]);
                    return arguments[1];
                }));

            functions.Add("set", new ScriptFunction("set", (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    var vName = arguments[1].ToString();
                    obj.SetProperty(vName, arguments[2]);
                    return arguments[2];
                }));

            functions.Add("eval", new ScriptFunction("eval", (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    return EvaluateString(context, thisObject, arguments[0].ToString());
                }));

            functions.Add("last", new ScriptFunction("last", (context, thisObject, arguments) =>
                {
                    ArgumentCountOrGreater(1, arguments);
                    return arguments[arguments.Count - 1];
                }));

            functions.Add("nop", new ScriptFunction("nop", (context, thisObject, arguments) => { return null; }));

            functions.Add("equal", new ScriptFunction("equal", (context, thisObject, arguments) =>
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
                        if (firstType == typeof(String)) { if (arguments[i].ToString() != arguments[i - 1].ToString()) return null; }
                        else if (firstType == typeof(int))
                        {
                            if ((arguments[i] as int?).Value != (arguments[i - 1] as int?).Value) return null;
                        }
                        else if (!Object.ReferenceEquals(arguments[i], arguments[i - 1])) return null;
                    }
                    return true;
                }));

            functions.Add("atleast", new ScriptFunction("atleast", (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    if (first.Value >= second.Value) return true;
                    return null;
                }));

            functions.Add("greaterthan", new ScriptFunction("greaterthan", (context, thisObject, arguments) =>
            {
                ArgumentCount(2, arguments);
                var first = arguments[0] as int?;
                var second = arguments[1] as int?;
                if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                if (first.Value > second.Value) return true;
                return null;
            }));

            functions.Add("nomorethan", new ScriptFunction("nomorethan", (context, thisObject, arguments) =>
            {
                ArgumentCount(2, arguments);
                var first = arguments[0] as int?;
                var second = arguments[1] as int?;
                if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                if (first.Value <= second.Value) return true;
                return null;
            }));

            functions.Add("lessthan", new ScriptFunction("lessthan", (context, thisObject, arguments) =>
            {
                ArgumentCount(2, arguments);
                var first = arguments[0] as int?;
                var second = arguments[1] as int?;
                if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                if (first.Value < second.Value) return true;
                return null;
            }));

            functions.Add("not", new ScriptFunction("not", (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    if (arguments[0] == null) return true;
                    else return null;
                }));

            functions.Add("coalesce", new ScriptFunction("coalesce", (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    if (arguments[0] == null) return arguments[1];
                    return arguments[0];
                }));
            #endregion

            #region Branching

            functions.Add("if", new ScriptFunction("if", (context, thisObject, arguments) =>
                {
                    if (arguments.Count != 2 && arguments.Count != 3) throw new ScriptError("If expects two or three arguments");
                    if (arguments[0] != null) return Evaluate(context, arguments[1] as Irony.Parsing.ParseTreeNode, thisObject, true);
                    else if (arguments.Count == 3) return Evaluate(context, arguments[2] as Irony.Parsing.ParseTreeNode, thisObject, true);
                    return null;
                }));

            #endregion

            #region List Manipulation Functions
            functions.Add("list", new ScriptFunction("list", (context, thisObject, arguments) =>
            {
                return arguments;
            }));

            functions.Add("length", new ScriptFunction("length", (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    var list = ArgumentType<List<Object>>(arguments[0]);
                    return list.Count;
                }));

            functions.Add("count", new ScriptFunction("count", (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var vName = arguments[0].ToString();
                    var list = ArgumentType<List<Object>>(arguments[1]);
                    var func = ArgumentType<Irony.Parsing.ParseTreeNode>(arguments[2]);

                    context.PushVariable(vName, null);
                    var result = list.Count((o) =>
                    {
                        context.ChangeVariable(vName, o);
                        return Evaluate(context, func, thisObject, true) != null;
                    });
                    context.PopVariable(vName);
                    return result;
                }));

            functions.Add("cat", new ScriptFunction("cat", (context, thisObject, arguments) =>
                {
                    var result = new List<Object>();
                    foreach (var arg in arguments)
                    {
                        if (arg is List<Object>) result.AddRange(arg as List<Object>);
                        else result.Add(arg);
                    }
                    return result;
                }));

            functions.Add("map", new ScriptFunction("map", (context, thisObject, arguments) =>
                {
                    var vName = arguments[0].ToString();
                    var list = ArgumentType<List<Object>>(arguments[1]);
                    var code = ArgumentType<Irony.Parsing.ParseTreeNode>(arguments[2]);
                    var result = new List<Object>();
                    context.PushVariable(vName, null);
                    foreach (var item in list)
                    {
                        context.ChangeVariable(vName, item);
                        result.Add(Evaluate(context, code, thisObject, true));
                    }
                    context.PopVariable(vName);
                    return result;
                }));


            functions.Add("for", new ScriptFunction("for", (context, thisObject, arguments) =>
                {
                    var vName = arguments[0].ToString();

                    var list = arguments[1] as List<Object>;
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

            functions.Add("where", new ScriptFunction("where", (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var vName = arguments[0].ToString();
                    var list = ArgumentType<List<Object>>(arguments[1]);
                    var func = ArgumentType<Irony.Parsing.ParseTreeNode>(arguments[2]);

                    context.PushVariable(vName, null);
                    var result = new List<Object>(list.Where((o) =>
                        {
                            context.ChangeVariable(vName, o);
                            return Evaluate(context, func, thisObject, true) != null;
                        }));
                    context.PopVariable(vName);
                    return result;
                }));
            #endregion

            #region String Functions
            functions.Add("substr", new ScriptFunction("substr", (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var index = (arguments[1] as int?).Value;
                    return arguments[0].ToString().Substring(index);
                }));

            functions.Add("strcat", new ScriptFunction("strcat", (context, thisObject, arguments) =>
                {
                    var r = "";
                    foreach (var obj in arguments) if (obj == null) r += "null"; else r += obj.ToString();
                    return r;
                }));
            #endregion
        }
    }
}
