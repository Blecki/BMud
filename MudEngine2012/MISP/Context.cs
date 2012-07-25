﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012.MISP
{
    public class Context
    {
        private List<Dictionary<String, ScriptList>> variables = new List<Dictionary<String, ScriptList>>();
        public ParseNode currentNode = null;

        public DateTime executionStart;
        public bool limitExecutionTime = true;
        public TimeSpan allowedExecutionTime = TimeSpan.FromSeconds(10);

        public Action<String> trace = null;
        public int traceDepth = 0;

        public void Reset()
        {
            variables.Clear();
            PushScope();
            ResetTimer();
        }

        public void ResetTimer()
        {
            executionStart = DateTime.Now;
        }

        public Context() { Reset(); }

        public Dictionary<String, ScriptList> Scope { get { return variables[variables.Count - 1]; } }

        public void PushScope() { variables.Add(new Dictionary<string, ScriptList>()); }
        public void PopScope() { variables.RemoveAt(variables.Count - 1); }

        public bool HasVariable(String name)
        {
            return Scope.ContainsKey(name);
        }

        public void PushVariable(String name, Object value)
        {
            if (!HasVariable(name)) Scope.Add(name, new ScriptList());
            Scope[name].Add(value);
        }

        public void PopVariable(String name)
        {
            var list = Scope[name];
            list.RemoveAt(list.Count - 1);
            if (list.Count == 0)
                Scope.Remove(name);
        }

        public Object GetVariable(String name)
        {
            var list = Scope[name];
            return list[list.Count - 1];
        }

        public void ChangeVariable(String name, Object newValue)
        {
            if (!Scope.ContainsKey(name)) 
                throw new ScriptError("Variable does not exist.", null);
            var list = Scope[name];
            list.RemoveAt(list.Count - 1);
            list.Add(newValue);
        }
    }
}
