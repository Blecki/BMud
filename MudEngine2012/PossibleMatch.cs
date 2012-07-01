using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class PossibleMatch : ScriptObject
    {
        public CommandTokenizer.Token token = null;
        public Dictionary<String, Object> arguments = null;

        public PossibleMatch(CommandTokenizer.Token Next, String ArgumentName = null, Object Argument = null)
        {
            token = Next;
            arguments = new Dictionary<String, Object>();

            if (ArgumentName != null && Argument != null) arguments.Add(ArgumentName, Argument);
        }

        public PossibleMatch(Dictionary<String, Object> Arguments, CommandTokenizer.Token Next, String ArgumentName = null, Object Argument = null)
        {
            token = Next;
            arguments = new Dictionary<String, Object>(Arguments);

            if (ArgumentName != null && Argument != null)
            {
                if (arguments.ContainsKey(ArgumentName)) arguments[ArgumentName] = Argument;
                else arguments.Add(ArgumentName, Argument);
            }
        }

        public void Upsert(String Name, Object Value)
        {
            if (arguments.ContainsKey(Name)) arguments[Name] = Value;
            else arguments.Add(Name, Value);
        }

        object ScriptObject.GetProperty(string name)
        {
            if (name == "token") return token;
            if (arguments.ContainsKey(name)) return arguments[name];
            else throw new ScriptError("No property with name " + name + " on object.");
        }

        void ScriptObject.SetProperty(string name, Object value) 
        {
            if (name == "token") token = value as CommandTokenizer.Token;
            else Upsert(name, value); 
        }

        ScriptList ScriptObject.ListProperties()
        {
            var r = new ScriptList(arguments.Select((p) => { return p.Key; }));
            r.Add("token");
            return r;
        }
    }
}
