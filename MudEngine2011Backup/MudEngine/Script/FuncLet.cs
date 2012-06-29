using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class LET : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count != 2) throw new RuntimeErrorException("LET takes exactly two arguments.");
            var Name = Children[0].Execute(Context) as String;
            var Value = Children[1].Execute(Context);

            if (Name == null) throw new RuntimeErrorException("Name cannot be null.");
            Context.Variables.Upsert(Name, Value);
            return Value;
        }
    }
}
