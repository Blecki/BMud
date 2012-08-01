using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public enum ArgumentType
    {
        STRING,
        INTEGER,
        LIST,
        OBJECT,
        CODE,
        FUNCTION,
        ANYTHING,
    }

    public class ArgumentInfo
    {
        public String name;
        public ArgumentType type;
        public bool optional;
        public bool repeat;
        public bool notNull;

        public static List<ArgumentInfo> ParseArguments(params String[] args)
        {
            var list = new ScriptList();
            foreach (var arg in args) list.Add(arg);
            return ParseArguments(list);
        }

        public static List<ArgumentInfo> ParseArguments(ScriptList args)
        {
            var r = new List<ArgumentInfo>();
            foreach (var arg in args)
            {
                if (!(arg is String)) throw new ScriptError("Argument names must be strings.", null);
                var parts = (arg as String).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                String typeDecl = "";
                String semanticDecl = "";
                if (parts.Length == 2)
                {
                    typeDecl = parts[0];
                    semanticDecl = parts[1];
                }
                else if (parts.Length == 1)
                {
                    semanticDecl = parts[0];
                }
                else
                    throw new ScriptError("Invalid argument declaration", null);

                var argInfo = new ArgumentInfo();

                if (!String.IsNullOrEmpty(typeDecl))
                    argInfo.type = (Enum.Parse(typeof(ArgumentType), typeDecl.ToUpperInvariant()) as ArgumentType?).Value;
                else
                    argInfo.type = ArgumentType.ANYTHING;

                while (semanticDecl.StartsWith("?") || semanticDecl.StartsWith("+"))
                {
                    if (semanticDecl[0] == '?') argInfo.optional = true;
                    if (semanticDecl[0] == '+') argInfo.repeat = true;
                    semanticDecl = semanticDecl.Substring(1);
                }

                argInfo.name = semanticDecl;

                r.Add(argInfo);
            }

            return r;
        }

    }

    public class Function : ReflectionScriptObject
    {
        private Func<Context, ScriptObject, ScriptList, Object> implementation = null;
        public String name;
        public String shortHelp;
        public bool isLambda = false;
        public ScriptList closedValues = null;
        public List<ArgumentInfo> argumentInfo = null;

        public Function(String name, List<ArgumentInfo> arguments, String shortHelp, Func<Context, ScriptObject, ScriptList, Object> func)
        {
            implementation = func;
            this.name = name;
            this.shortHelp = shortHelp;
            this.argumentInfo = arguments;
        }

        private static T CheckArgumentType<T>(Object obj, Context context) where T : class
        {
            if (obj == null)
                throw new ScriptError("Expecting argument of type " + typeof(T) + ", got null. ", context.currentNode);
            var r = obj as T;
            if (r == null)
                throw new ScriptError("Function argument is the wrong type. Expected type "
                    + typeof(T) + ", got " + obj.GetType() + ". ", context.currentNode);
            return r;
        }

        private static void checkArgumentType(ArgumentType type, Object argument, Context context)
        {
            switch (type)
            {
                case ArgumentType.STRING:
                    CheckArgumentType<String>(argument, context);
                    break;
                case ArgumentType.OBJECT:
                    CheckArgumentType<ScriptObject>(argument, context);
                    break;
                case ArgumentType.LIST:
                    CheckArgumentType<ScriptList>(argument, context);
                    break;
                case ArgumentType.INTEGER:
                    if (argument == null) return;
                    if (!(argument is int)) throw new ScriptError("Argument is wrong type.", context.currentNode);
                    break;
                case ArgumentType.CODE:
                    CheckArgumentType<ParseNode>(argument, context);
                    break;
                case ArgumentType.FUNCTION:
                    if (argument == null) return;
                    CheckArgumentType<Function>(argument, context);
                    break;
                default:
                    break;
            }
        }

        public ArgumentInfo GetArgumentInfo(int index)
        {
            if (index >= argumentInfo.Count)
            {
                if (argumentInfo.Count > 0 && argumentInfo[argumentInfo.Count - 1].repeat)
                    return argumentInfo[argumentInfo.Count - 1];
                else
                    throw new ScriptError("Argument out of bounds", null);
            }
            else
                return argumentInfo[index];
        }

        public Object Invoke(Context context, ScriptObject thisObject, ScriptList arguments)
        {
            if (context.trace != null)
            {
                context.trace(new String('.', context.traceDepth) + "Entering " + name + " ( " + String.Join(", ", arguments.Select((o) => ScriptObject.AsString(o, 2))) + " )\n");
                if (closedValues != null && closedValues.Count > 0) context.trace(new String('.', context.traceDepth) + " closed: " + ScriptObject.AsString(closedValues, 2) + "\n");
                context.traceDepth += 1;
            }

            var newArguments = new ScriptList();
            //Check argument types
            if (argumentInfo.Count == 0 && arguments.Count != 0) throw new ScriptError("Function expects no arguments.", context.currentNode);


            int argumentIndex = 0;               
            for (int i = 0; i < argumentInfo.Count; ++i)
            {
                var info = argumentInfo[i];

                if (info.repeat)
                {
                    var list = new ScriptList();
                    while (argumentIndex < arguments.Count) //Handy side effect: If no argument is passed for an optional repeat
                    {                                       //argument, it will get an empty list.
                        list.Add(ProcessArgument(info, arguments[argumentIndex], context));
                        ++argumentIndex;
                    }
                    newArguments.Add(list);
                }
                else
                {
                    if (argumentIndex < arguments.Count)
                        newArguments.Add(ProcessArgument(info, arguments[argumentIndex], context));
                    else if (info.optional)
                        newArguments.Add(createDefaultArgument(info.type));
                    else throw new ScriptError("Not enough arguments to " + name, context.currentNode);
                    ++argumentIndex;
                }
            }
            if (argumentIndex < arguments.Count)
                throw new ScriptError("Too many arguments to " + name, context.currentNode);

            var r = implementation(context, thisObject, newArguments);

            if (context.trace != null)
            {
                context.traceDepth -= 1;
                context.trace(new String('.', context.traceDepth) + "Leaving " + name + "\n");
            }

            return r;
        }
        
        private Object ProcessArgument(ArgumentInfo info, Object argument, Context context)
        {
            if (argument == null) return createDefaultArgument(info.type);
            if (info.type == ArgumentType.LIST && !(argument is ScriptList))
                return new ScriptList(argument);
            if (info.type == ArgumentType.STRING && !(argument is String))
                return ScriptObject.AsString(argument);
            else checkArgumentType(info.type, argument, context);
            return argument;
        }
        
        private object createDefaultArgument(ArgumentType argumentType)
        {
            switch (argumentType)
            {
                case ArgumentType.STRING:
                    return "";
                case ArgumentType.OBJECT:
                    return null;
                case ArgumentType.LIST:
                    return new ScriptList();
                case ArgumentType.INTEGER:
                    return null;
                case ArgumentType.CODE:
                    return null;
                case ArgumentType.FUNCTION:
                    return null;
                case ArgumentType.ANYTHING:
                    return null;
                default:
                    return null;
            }
        }

    }
}
