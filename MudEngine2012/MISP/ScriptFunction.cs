using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012.MISP
{
    public class ScriptFunction : ReflectionScriptObject
    {
        private Func<ScriptContext, ScriptObject, ScriptList, Object> implementation = null;
        public String name;
        public String shortHelp;
        public bool isLambda = false;
        public ScriptList closedValues = null;

        public ScriptFunction(String name, String shortHelp, Func<ScriptContext, ScriptObject, ScriptList, Object> func)
        {
            implementation = func;
            this.name = name;
            this.shortHelp = shortHelp;
        }

        public Object Invoke(ScriptContext context, ScriptObject thisObject, ScriptList arguments)
        {
            if (context.trace != null)
            {
                context.trace(new String('.', context.traceDepth) + "Entering " + name + " ( " + String.Join(", ", arguments.Select((o) => ScriptObject.AsString(o, 2))) + " )\n");
                if (closedValues != null && closedValues.Count > 0) context.trace(new String('.', context.traceDepth) + " closed: " + ScriptObject.AsString(closedValues, 2) + "\n");
                context.traceDepth += 1;
            }

            var r = implementation(context, thisObject, arguments);

            if (context.trace != null)
            {
                context.traceDepth -= 1;
                context.trace(new String('.', context.traceDepth) + "Leaving " + name + "\n");
            }

            return r;
        }

    }
}
