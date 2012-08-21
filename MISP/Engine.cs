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
        private Dictionary<String, Function> functions = new Dictionary<String, Function>();
        private Dictionary<String, Func<Context, Object>> specialVariables
            = new Dictionary<string, Func<Context, object>>();
        internal Dictionary<String, Type> types = new Dictionary<string, Type>();

        public static T ArgumentType<T>(Object obj) where T : class
        {
            return obj as T;
        }
        
        public Engine()
        {
            this.SetupStandardLibrary();
        }

        public void AddFunction(String name, String comment, Func<Context, ScriptList, Object> func,
            params String[] arguments)
        {
            functions.Add(name, new Function(name,
                ArgumentInfo.ParseArguments(this, arguments),
                comment,
                func));
        }

        public Function MakeLambda(String name, String comment, Func<Context, ScriptList, Object> func,
            params String[] arguments)
        {
            return new Function(name, ArgumentInfo.ParseArguments(this, arguments), comment, func);
        }

        public void AddGlobalVariable(String name, Func<Context, Object> getFunc)
        {
            specialVariables.Add(name, getFunc);
        }

        public Object EvaluateString(Context context, String str, String fileName, bool discardResults = false)
        {
            var root = Parser.ParseRoot(str, fileName);
            return Evaluate(context, root, false, discardResults);
        }

        public Object Evaluate(
            Context context,
            Object what,
            bool ignoreStar = false,
            bool discardResults = false)
        {
            if (what is String) return EvaluateString(context, what as String, "", discardResults);
            else if (!(what is ParseNode)) return what;

            var node = what as ParseNode;
            context.currentNode = node;

            if (context.limitExecutionTime && (DateTime.Now - context.executionStart > context.allowedExecutionTime))
                throw new TimeoutError(node);

            if (node.prefix == Prefix.Quote && !ignoreStar) return node;
            object result = null;

            switch (node.type)
            {
                case NodeType.String:
                    result = node.token;
                    break;
                case NodeType.StringExpression:
                    if (discardResults) //Don't bother assembling the string expression.
                    {
                        foreach (var piece in node.childNodes)
                        {
                            if (piece.type == NodeType.String)
                                continue;
                            else
                                Evaluate(context, piece);
                        }
                        result = null;
                    }
                    else
                    {
                        if (node.childNodes.Count == 1) //If there's only a single item, the result is that item.
                            result = Evaluate(context, node.childNodes[0]);
                        else
                        {
                            var resultString = String.Empty;
                            foreach (var piece in node.childNodes)
                                resultString += ScriptObject.AsString(Evaluate(context, piece));
                            result = resultString;
                        }
                    }
                    break;
                case NodeType.Token:
                    result = LookupToken(context, node.token);
                    break;
                case NodeType.MemberAccess:
                    {
                        //SET THIS BEFORE EVALUATING MEMBER
                        var lhs = Evaluate(context, node.childNodes[0]);
                        String rhs = "";
                        if (node.childNodes[1].type == NodeType.Token)
                            rhs = node.childNodes[1].token;
                        else
                            rhs = ScriptObject.AsString(Evaluate(context, node.childNodes[1], false));

                        if (lhs == null) result = null;
                        else if (lhs is ScriptObject)
                        {
                            result = (lhs as ScriptObject).GetProperty(ScriptObject.AsString(rhs));
                            if (node.token == ":")
                            {
                                context.Scope.PushVariable("this", lhs);
                                result = Evaluate(context, result, true, false);
                                context.Scope.PopVariable("this");
                            }
                        }
                        //    else
                        //{
                        //    try {
                        //        var member = lhs.GetType().GetMember(ScriptObject.AsString(rhs));
                        //        if (member.Length == 0) result = null;
                                
                        else
                            result = null;
                    }
                    break;
                case NodeType.Node:
                    {
                        if (!ignoreStar && node.prefix == Prefix.Quote)
                        {
                            result = node;
                            break;
                        }

                        bool eval = node.prefix != Prefix.List;

                        var arguments = new ScriptList();

                        try
                        {
                            foreach (var child in node.childNodes)
                            {
                                bool argumentProcessed = false;

                                if (eval && arguments.Count > 0 && (arguments[0] is Function)) //This is a function call
                                {
                                    var func = arguments[0] as Function;
                                    var argumentInfo = func.GetArgumentInfo(arguments.Count - 1);
                                    if (argumentInfo.type == MISP.ArgumentInfo.CodeType)
                                    {
                                        if (child.prefix == Prefix.Evaluate || child.prefix == Prefix.Lookup)
                                        {
                                            //Some prefixs override special behavior of code type.
                                            arguments.Add(Evaluate(context, child, true));
                                        }
                                        else if (child.prefix == Prefix.Quote || child.prefix == Prefix.None)
                                            arguments.Add(child);
                                        else if (child.prefix == Prefix.List)
                                        {
                                            //raise warning
                                            arguments.Add(child);
                                        }
                                        else throw new ScriptError("Prefix invalid in this context.", child);
                                        argumentProcessed = true;
                                    }
                                }

                                if (!argumentProcessed)
                                {
                                    var argument = Evaluate(context, child);
                                    if (child.prefix == Prefix.Expand && argument is ScriptList)
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

                        if (node.prefix == Prefix.List) result = arguments;
                        else
                        {
                            if (arguments.Count > 0 && arguments[0] is Function)
                            {
                                try
                                {
                                    result = (arguments[0] as Function).Invoke(this, context,
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
                            else if (arguments.Count > 0)
                                result = arguments[0];
                            else
                                result = null;
                        }
                    }
                    break;
                case NodeType.Number:
                    if (node.token.Contains('.')) result = Convert.ToSingle(node.token);
                    else result = Convert.ToInt32(node.token);
                    break;
                case NodeType.DictionaryEntry:
                    {
                        var r = new ScriptList();
                        foreach (var child in node.childNodes)
                            if (child.type == NodeType.Token) r.Add(child.token);
                            else r.Add(Evaluate(context, child));
                        result = r;
                    }
                    break;
                default:

                    throw new ScriptError("Internal evaluator error", node);
            }

            if (node.prefix == Prefix.Evaluate && !ignoreStar)
                result = Evaluate(context, result);
            if (node.prefix == Prefix.Lookup) result = LookupToken(context, ScriptObject.AsString(result));

            return result;
        }

        private object LookupToken(Context context, String value)
        {
            value = value.ToLowerInvariant();
            if (specialVariables.ContainsKey(value)) return specialVariables[value](context);
            if (context.Scope.HasVariable(value)) return context.Scope.GetVariable(value);
            if (functions.ContainsKey(value)) return functions[value];
            if (value.StartsWith("@") && functions.ContainsKey(value.Substring(1))) return functions[value.Substring(1)];
            throw new ScriptError("Could not find value with name " + value + ".", context.currentNode);
        }
    }
}
