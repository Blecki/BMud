using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class Client : ScriptObject
    {
        public virtual void Send(String message) { }
        public virtual void Disconnect() { }

        internal MudObject player;
        internal bool logged_on;

        public override Object GetProperty(String name) 
        {
            if (name == "player") return player;
            if (name == "logged_on") return logged_on;
            return null;
        }

        public override void SetProperty(String name, Object value)
        {
            if (name == "player") player = value as MudObject;
            if (name == "logged_on") logged_on = (value != null);
        }

        public override void DeleteProperty(String name) { }

        public override ScriptList ListProperties() { return new ScriptList(new Object[] { "player", "logged_on" }); }

    }
}
