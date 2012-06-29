using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class REPLACE : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count != 3) throw new RuntimeErrorException("REPLACE takes 3 arguments.");
            var String = Children[0].Execute(Context) as String;
            var What = Children[1].Execute(Context) as String;
            var With = Children[2].Execute(Context);

            if (String == null || What == null || With == null) throw new RuntimeErrorException("Type error in REPLACE.");

            String _with = "";
            if (With is String) _with = With as String;
            else if (With is Integer) _with = (With as Integer).Value.ToVerbal();

            return String.Replace(What, _with);
        }
    }
}
