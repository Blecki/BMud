using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public enum EvaluationState
    {
        Normal,
        UnwindingError,
    }

    public class Context
    {
        private List<Scope> scopeStack = new List<Scope>();
        public ParseNode currentNode = null;
        public EvaluationState evaluationState;
        public ScriptObject errorObject = null;

        public void RaiseNewError(String message, ParseNode location)
        {
            errorObject = new GenericScriptObject("message", message, "location", location, "stack-trace", new ScriptList());
            evaluationState = EvaluationState.UnwindingError;
        }

        public void PushStackTrace(String message)
        {
            (errorObject.GetProperty("stack-trace") as ScriptList).Add(message);
        }

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
            evaluationState = EvaluationState.Normal;
        }

        public void ResetTimer()
        {
            executionStart = DateTime.Now;
        }

        public Context(params object[] vars)
        { 
            Reset();
            for (var i = 0; i < vars.Length; i += 2)
            {
                if (i + 1 >= vars.Length) throw new InvalidProgramException();
                Scope.PushVariable(vars[i].ToString(), vars[i + 1]);
            }
        }


        public Scope Scope { get { return scopeStack.Count > 0 ? scopeStack[scopeStack.Count - 1] : null; } }

        public void PushScope(Scope scope) { scope.parentScope = Scope; scopeStack.Add(scope); }
        public void PopScope() { Scope.parentScope = null; scopeStack.RemoveAt(scopeStack.Count - 1); }

       
    }
}
