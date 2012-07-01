using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class ScriptContext
    {
        public MudObject actor;
        private Dictionary<String, ScriptList> variables = new Dictionary<String, ScriptList>();
        public DateTime executionStart;
        public String activeSource { get; set; }

        public void Reset(MudObject actor)
        {
            this.actor = actor;
            variables.Clear();
            executionStart = DateTime.Now;
        }

        public ScriptContext() { Reset(null); }

        public bool HasVariable(String name)
        {
            return variables.ContainsKey(name);
        }

        public void PushVariable(String name, Object value)
        {
            if (!HasVariable(name)) variables.Add(name, new ScriptList());
            variables[name].Add(value);
        }

        public void PopVariable(String name)
        {
            var list = variables[name];
            list.RemoveAt(list.Count - 1);
            if (list.Count == 0)
                variables.Remove(name);
        }

        public Object GetVariable(String name)
        {
            var list = variables[name];
            return list[list.Count - 1];
        }

        public void ChangeVariable(String name, Object newValue)
        {
            var list = variables[name];
            list.RemoveAt(list.Count - 1);
            list.Add(newValue);
        }
    }
}
