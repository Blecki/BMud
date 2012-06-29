using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class FETCH : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count < 2) throw new RuntimeErrorException("Wrong number of arguments to FETCH.");
            if (Children.Count > 3) throw new RuntimeErrorException("Wrong number of arguments to FETCH.");

            var What = Children[0].Execute(Context) as MudObject;
            var Key = Children[1].Execute(Context) as String;
            var Default = (Children.Count == 3 ? Children[2].Execute(Context) as String : "");

            if (What == null || Key == null || Default == null) throw new RuntimeErrorException("Type mismatch in FETCH.");
            return What.GetAttribute(Key, Default);
        }
    }

    internal class CONTENTS : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count != 2) throw new RuntimeErrorException("Wrong number of arguments to CONTENTS.");

            var List = Children[0].Execute(Context) as String;
            var What = Children[1].Execute(Context) as MudObject;

            if (What == null || List == null) throw new RuntimeErrorException("Type mismatch in CONTENTS.");
            return What.GetContents(List);
        }
    }
}
