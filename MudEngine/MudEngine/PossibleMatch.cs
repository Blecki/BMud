using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public class PossibleMatch
    {
        public LinkedListNode<CommandParser.Token> _next = null;
        public Dictionary<String, Object> _arguments = null;

        public T GetArgument<T>(String Name, T DefaultValue)
        {
            if (!_arguments.ContainsKey(Name)) return DefaultValue;
            else return (T)_arguments[Name];
        }

        public Object GetRawArgument(String Name)
        {
            if (!_arguments.ContainsKey(Name)) return null;
            else return _arguments[Name];
        }

        public List<MudObject> GetArgumentAsObjectList(String Name)
        {
            var Object = GetRawArgument(Name);
            if (Object is List<MudObject>) return Object as List<MudObject>;
            if (Object is MudObject) return new List<MudObject>{Object as MudObject};
            return new List<MudObject>();
        }

        public PossibleMatch(LinkedListNode<CommandParser.Token> Next, String ArgumentName = null, Object Argument = null)
        {
            _next = Next;
            _arguments = new Dictionary<String, Object>();

            if (ArgumentName != null && Argument != null) _arguments.Add(ArgumentName, Argument);
        }

        public PossibleMatch(Dictionary<String,Object> Arguments, LinkedListNode<CommandParser.Token> Next, String ArgumentName = null, Object Argument = null)
        {
            _next = Next;
            _arguments = new Dictionary<String, Object>(Arguments);

            if (ArgumentName != null && Argument != null)
            {
                if (_arguments.ContainsKey(ArgumentName)) _arguments[ArgumentName] = Argument;
                else _arguments.Add(ArgumentName, Argument);
            }
        }

        public void Upsert(String Name, Object Value)
        {
            if (_arguments.ContainsKey(Name)) _arguments[Name] = Value;
            else _arguments.Add(Name, Value);
        }
    }
}
