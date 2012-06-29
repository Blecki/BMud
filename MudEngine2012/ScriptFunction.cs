using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class ScriptFunction : ScriptObject
    {
        private Func<ScriptContext, ScriptObject, List<Object>, Object> implementation = null;
        public String name { get; private set; }

        public ScriptFunction(String name, Func<ScriptContext, ScriptObject, List<Object>, Object> func)
        {
            implementation = func;
            this.name = name;
        }

        public Object Invoke(ScriptContext context, ScriptObject thisObject, List<Object> arguments)
        {
            return implementation(context, thisObject, arguments);
        }

        object ScriptObject.GetProperty(string name)
        {
            if (name == "name") return this.name;
            throw new ScriptError(name + " is not a property of ScriptFunction");
        }

        void ScriptObject.SetProperty(string name, object value)
        {
            throw new ScriptError("ScriptFunction is read-only");
        }
    }
}
