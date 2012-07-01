﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class ScriptFunction : ReflectionScriptObject
    {
        private Func<ScriptContext, ScriptObject, ScriptList, Object> implementation = null;
        public String name;
        public String source;
        public String shortHelp;

        public ScriptFunction(String name, String shortHelp, Func<ScriptContext, ScriptObject, ScriptList, Object> func)
        {
            implementation = func;
            this.name = name;
            this.shortHelp = shortHelp;
        }

        public Object Invoke(ScriptContext context, ScriptObject thisObject, ScriptList arguments)
        {
            //try
            //{
                return implementation(context, thisObject, arguments);
            //}
            //catch (ScriptError e)
            //{
            //    throw new ScriptError(source + " " + e.Message);
            //}
        }

    }
}
