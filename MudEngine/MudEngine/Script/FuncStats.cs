using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class GRANT : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count != 3) throw new RuntimeErrorException("GRANT takes 3 arguments.");
            var Who = Children[0].Execute(Context) as MudObject;
            var Stat = Children[1].Execute(Context) as String;
            var Amount = Children[2].Execute(Context) as Integer;

            if (Who == null || Stat == null || Amount == null) throw new RuntimeErrorException("Type error in GRANT.");
            MudCore.GrantStat(Who, Stat, Amount.Value);
            return Parser.True;
        }
    }
}
