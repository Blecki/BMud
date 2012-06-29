using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class LOCATION : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Context.OperationLimit.Dec()) throw new OperationLimitExceededException();
            if (Children.Count != 1) throw new RuntimeErrorException("LOCATION only takes 1 argument.");
            var Of = Children[0].Execute(Context) as MudObject;
            if (Of == null) return null;
            return Of.Location.Parent;
        }
    }
}
