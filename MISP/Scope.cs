using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public class Scope : ScriptObject
    {
        internal Scope parentScope = null;
        internal Dictionary<String, ScriptList> variables = new Dictionary<String, ScriptList>();

        public override Object GetProperty(String name) { return GetVariable(name);  }
        public override Object GetLocalProperty(String name) { return GetVariable(name); }
        public override void SetProperty(String name, Object value) 
        { 
            if (HasVariable(name)) ChangeVariable(name, value); 
            else PushVariable(name, value); 
        }
        public override void DeleteProperty(String name) { if (HasVariable(name)) variables.Remove(name.ToLowerInvariant()); }
        public override ScriptList ListProperties() { return new ScriptList(variables.Select((p) => p.Key)); }
        public override void ClearProperties() { variables.Clear(); }

        public bool HasVariable(String name)
        {
            return variables.ContainsKey(name.ToLowerInvariant());
        }

        public void PushVariable(String name, Object value)
        {
            name = name.ToLowerInvariant();
            if (!HasVariable(name)) variables.Add(name, new ScriptList());
            variables[name].Add(value);
        }

        public void PopVariable(String name)
        {
            name = name.ToLowerInvariant();
            if (!HasVariable(name)) return;
            var list = variables[name.ToLowerInvariant()];
            list.RemoveAt(list.Count - 1);
            if (list.Count == 0)
                variables.Remove(name);
        }

        public Object GetVariable(String name)
        {
            name = name.ToLowerInvariant();
            if (name == "@parent") return parentScope;
            if (!HasVariable(name)) return null;
            var list = variables[name];
            return list[list.Count - 1];
        }

        public void ChangeVariable(String name, Object newValue)
        {
            name = name.ToLowerInvariant();
            if (!variables.ContainsKey(name)) 
                throw new ScriptError("Variable does not exist.", null);
            var list = variables[name];
            list.RemoveAt(list.Count - 1);
            list.Add(newValue);
        }
    }
}
