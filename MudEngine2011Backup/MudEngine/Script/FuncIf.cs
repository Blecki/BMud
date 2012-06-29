using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class IF : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Context.OperationLimit.Dec()) throw new OperationLimitExceededException();
            if (Children.Count < 2) throw new RuntimeErrorException("IF needs at least 2 arguments.");
            if (Children.Count > 3) throw new RuntimeErrorException("IF takes only 3 arguments.");

            var Conditional = Children[0].Execute(Context);
            if (Conditional != null) return Children[1].Execute(Context);
            else if (Children.Count == 3) return Children[2].Execute(Context);
            else return null;
        }
    }
}
