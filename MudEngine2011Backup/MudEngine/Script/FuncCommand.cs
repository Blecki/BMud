using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class COMMAND : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count != 2) throw new RuntimeErrorException("COMMAND takes 2 arguments.");
            var Who = Children[0].Execute(Context) as MudObject;
            var Text = Children[1].Execute(Context) as String;

            if (MudCore.CheckPermission(Context.Executor, Who, Context._databaseService))
            {
                Context._messageService.Command(Who.ID, Text);
                return Parser.True;
            }
            return null;
        }
    }
}
