using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public class Type
    {
        public virtual Object ProcessArgument(Object obj) { return obj; }
        public virtual Object CreateDefault() { return null; }
        public static Type Anything = new Type();
    }

    public class TypeGeneric : Type
    {
        private System.Type type;
        private bool allowNull;
        public TypeGeneric(System.Type type, bool allowNull) { this.type = type; this.allowNull = allowNull; }
        public override object ProcessArgument(object obj)
        {
            if (allowNull && obj == null) return obj;
            try
            {
                Function.CheckArgumentType(obj, type);
            }
            catch (ScriptError e)
            {
                try
                {
                    return Convert.ChangeType(obj, type);
                }
                catch (Exception exp)
                {
                    throw e;
                }
            }
            return obj;
        }
        public override object CreateDefault()
        {
            return null;
        }
    }

    public class TypeString : Type
    {
        public override object ProcessArgument(object obj)
        {
            if (obj == null) return "";
            else return ScriptObject.AsString(obj);
        }

        public override object CreateDefault()
        {
            return "";
        }
    }

    public class TypeList : Type
    {
        public override object ProcessArgument(object obj)
        {
            if (obj == null) return new ScriptList();
            else if (!(obj is ScriptList)) return new ScriptList(obj);
            return obj;
        }

        public override object CreateDefault()
        {
            return new ScriptList();
        }
    }
    
    public class ArgumentInfo
    {
        public String name;
        //public ArgumentType type;
        public bool optional;
        public bool repeat;
        public bool notNull;
        public Type type;

        public static TypeGeneric CodeType = new TypeGeneric(typeof(MISP.ParseNode), false);

        public static List<ArgumentInfo> ParseArguments(Engine engine, params String[] args)
        {
            var list = new ScriptList();
            foreach (var arg in args) list.Add(arg);
            return ParseArguments(engine, list);
        }

        public static List<ArgumentInfo> ParseArguments(Engine engine, ScriptList args)
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
                {
                    if (engine.types.ContainsKey(typeDecl.ToUpperInvariant()))
                        argInfo.type = engine.types[typeDecl.ToUpperInvariant()];
                    else
                        throw new ScriptError("Unknown type " + typeDecl, null);
                }
                else
                    argInfo.type = Type.Anything;

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
        public ScriptList closedValues = null;
        public List<ArgumentInfo> argumentInfo = null;
        public Scope declarationScope;
        public bool isSystem = true;
        public Object body = null;

        public Function(
            String name,
            List<ArgumentInfo> arguments, 
            String shortHelp, 
            Func<Context, ScriptObject, ScriptList, Object> func)
        {
            body = null;
            isSystem = true;
            implementation = func;
            this.name = name;
            this.shortHelp = shortHelp;
            this.argumentInfo = arguments;
            this.declarationScope = new Scope();
        }

        public Function(
            String name,
            List<ArgumentInfo> arguments,
            String shortHelp,
            Object body,
            Scope declarationScope)
        {
            implementation = null;
            isSystem = false;
            this.body = body;
            this.name = name;
            this.shortHelp = shortHelp;
            this.argumentInfo = arguments;
            this.declarationScope = declarationScope ?? new Scope();
        }

        public static void CheckArgumentType(Object obj, System.Type type)
        {
            if (obj == null)
                throw new ScriptError("Expecting argument of type " + type + ", got null. ", null);
            if (obj.GetType() == type || obj.GetType().IsSubclassOf(type)) return;
            throw new ScriptError("Function argument is the wrong type. Expected type "
                    + type + ", got " + obj.GetType() + ". ", null);
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

        public Object Invoke(Engine engine, Context context, ScriptObject thisObject, ScriptList arguments)
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
                        list.Add(info.type.ProcessArgument(arguments[argumentIndex]));
                        ++argumentIndex;
                    }
                    newArguments.Add(list);
                }
                else
                {
                    if (argumentIndex < arguments.Count)
                        newArguments.Add(info.type.ProcessArgument(arguments[argumentIndex]));
                    else if (info.optional)
                        newArguments.Add(info.type.CreateDefault());
                    else throw new ScriptError("Not enough arguments to " + name, context.currentNode);
                    ++argumentIndex;
                }
            }
            if (argumentIndex < arguments.Count)
                throw new ScriptError("Too many arguments to " + name, context.currentNode);

            Object r = null;

            if (isSystem)
            {
                r = implementation(context, thisObject, newArguments);
            }
            else
            {
                context.PushScope(declarationScope);
                
                for (int i = 0; i < argumentInfo.Count; ++i)
                    context.Scope.PushVariable(argumentInfo[i].name, newArguments[i]);

                r = engine.Evaluate(context, body, thisObject, true);

                for (int i = 0; i < argumentInfo.Count; ++i)
                    context.Scope.PopVariable(argumentInfo[i].name);

                context.PopScope();
            }

            if (context.trace != null)
            {
                context.traceDepth -= 1;
                context.trace(new String('.', context.traceDepth) + "Leaving " + name + "\n");
            }

            return r;
        }
        
        

    }
}
