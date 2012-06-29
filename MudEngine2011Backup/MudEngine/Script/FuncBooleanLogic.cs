using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class IS : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Context.OperationLimit.Dec()) throw new OperationLimitExceededException();
            if (Children.Count != 2) throw new RuntimeErrorException("IS takes exactly two arguments.");
            var A = Children[0].Execute(Context);
            var B = Children[1].Execute(Context);

            if (A is MudObject && B is MudObject)
            {
                if ((A as MudObject).ID == (B as MudObject).ID) return Parser.True;
                else return null;
            }

            if (A is String && B is String)
            {
                if ((A as String) == (B as String)) return Parser.True;
                else return null;
            }

            if (A is Integer && B is Integer)
            {
                if ((A as Integer).Value == (B as Integer).Value) return Parser.True;
                else return null;
            }

            return null;           
        }
    }

    internal class NOT : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Context.OperationLimit.Dec()) throw new OperationLimitExceededException();
            if (Children.Count != 1) throw new RuntimeErrorException("NOT takes exactly one argument.");
            var A = Children[0].Execute(Context);
            if (A == null) return Parser.True;
            else return null;
        }
    }
}
