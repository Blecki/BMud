using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class Client : MISP.ReflectionScriptObject
    {
        public virtual void Send(String message) { }
        public virtual void Disconnect() { }

        public MISP.ScriptObject player;
        public bool logged_on;
    }
}
