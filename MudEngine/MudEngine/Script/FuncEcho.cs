using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class ECHO : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count != 2) throw new RuntimeErrorException("ECHO takes 2 arguments.");
            Object rawWho = Children[0].Execute(Context);
            var Text = Children[1].Execute(Context) as String;
            if (rawWho == null || Text == null) throw new RuntimeErrorException("Type error in ECHO.");

            if (rawWho is MudObject)
                Context._messageService.SendMessage(rawWho as MudObject, Text);
            else if (rawWho is List<MudObject>)
                foreach (var Who in (rawWho as List<MudObject>))
                    Context._messageService.SendMessage(Who, Text);
            return Parser.True;
        }
    }
}
