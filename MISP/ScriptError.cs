using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public class ScriptError : Exception 
    {
        public ParseNode generatedAt = null;

        public ScriptError(String msg, ParseNode generatedAt) : base(msg)
        {
            this.generatedAt = generatedAt;
        } 
    }
}
