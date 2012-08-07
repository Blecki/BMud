using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public class Context
    {
        private List<Scope> scopeStack = new List<Scope>();
        public ParseNode currentNode = null;

        public DateTime executionStart;
        public bool limitExecutionTime = true;
        public TimeSpan allowedExecutionTime = TimeSpan.FromSeconds(10);

        public Action<String> trace = null;
        public int traceDepth = 0;

        public void Reset()
        {
            scopeStack.Clear();
            PushScope(new Scope());
            ResetTimer();
        }

        public void ResetTimer()
        {
            executionStart = DateTime.Now;
        }

        public Context() { Reset(); }

        public Scope Scope { get { return scopeStack[scopeStack.Count - 1]; } }

        public void PushScope(Scope scope) { scopeStack.Add(scope); }
        public void PopScope() { scopeStack.RemoveAt(scopeStack.Count - 1); }

       
    }
}
