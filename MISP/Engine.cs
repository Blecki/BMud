using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public class TimeoutError : ScriptError
    {
        public TimeoutError(ParseNode generatedBy) : base("Execution timed out.", generatedBy) { }
    }

    public partial class Engine
    {
        public Dictionary<String, Function> functions = new Dictionary<String, Function>();
        public Dictionary<String, Func<Context, ScriptObject, Object>> specialVariables
            = new Dictionary<string, Func<Context, ScriptObject, object>>();

        public static T ArgumentType<T>(Object obj) where T : class
        {
            return obj as T;
        }
        
        public Engine()
        {
            this.SetupStandardLibrary();
        }

        public void AddFunction(String name, String comment, Func<Context, ScriptObject, ScriptList, Object> func,
            params String[] arguments)
        {
            functions.Add(name, new Function(name,
                ArgumentInfo.ParseArguments(arguments),
                comment,
                func));
        }

        public Object EvaluateString(Context context, ScriptObject thisObject, String str, String fileName, bool discardResults = false)
        {
            var root = Parser.ParseRoot(str, fileName);
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
                throw new TimeoutError(node);
            context.currentNode = node;

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
                                result = EvaluateString(context, lhs as ScriptObject, result as String, "");
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
                        catch (ScriptError e)
                        {
                            if (arguments.Count > 0 && arguments[0] is Function)
                                throw new ScriptError("[Arg for " + (arguments[0] as Function).name + "] " + e.Message,
                                    e.generatedAt == null ? node : e.generatedAt);
                            throw e;
                        }
                        catch (Exception e)
                        {
                            if (arguments.Count > 0 && arguments[0] is Function)
                                throw new ScriptError("[Arg for " + (arguments[0] as Function).name + "] " + e.Message, node);
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
                            catch (ScriptError e)
                            {
                                throw new ScriptError("[" + (arguments[0] as Function).name + "] " + e.Message,
                                    e.generatedAt == null ? node : e.generatedAt);
                            }
                            catch (Exception e)
                            {
                                throw new ScriptError("[" + (arguments[0] as Function).name + "] " + e.Message, node);
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

                    throw new ScriptError("Internal evaluator error", node);
            }
        }

        private object LookupToken(Context context, String value, ScriptObject thisObject)
        {
            value = value.ToLowerInvariant();
            if (specialVariables.ContainsKey(value)) return specialVariables[value](context, thisObject);
            if (context.HasVariable(value)) return context.GetVariable(value);
            if (functions.ContainsKey(value)) return functions[value];
            if (value.StartsWith("@") && functions.ContainsKey(value.Substring(1))) return functions[value.Substring(1)];
            throw new ScriptError("Could not find value with name " + value + ".", context.currentNode);
        }
    }
}
