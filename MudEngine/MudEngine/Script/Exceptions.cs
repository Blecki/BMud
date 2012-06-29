using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    public class ScriptException : Exception
    {
        public ScriptException(String message) : base(message) { }
    }

    public class SyntaxErrorException : ScriptException
    {
        public SyntaxErrorException(String message) : base("Syntax Error: " + message) { }
    }

    public class RuntimeErrorException : ScriptException
    {
        public RuntimeErrorException(String message) : base("Runtime Error: " + message) { }
    }
}
