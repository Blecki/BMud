using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class EVAL : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count == 4)
            {
                var Actor = Children[0].Execute(Context) as MudObject;
                var Me = Children[1].Execute(Context) as MudObject;
                var Object = Children[2].Execute(Context) as MudObject;
                var Text = Children[3].Execute(Context) as String;
                return Evaluator.EvaluateStringEx(Actor, Me, Object,
                Text, Context._databaseService, Context.OperationLimit);
            }
            else if (Children.Count == 3)
            {
                var Actor = Children[0].Execute(Context) as MudObject;
                var Me = Children[1].Execute(Context) as MudObject;
                var Text = Children[2].Execute(Context) as String;
                return Evaluator.EvaluateStringEx(Actor, Me, Me,
                                Text, Context._databaseService, Context.OperationLimit);
            }
            else if (Children.Count == 2)
            {
                var Actor = Children[0].Execute(Context) as MudObject;
                var Text = Children[1].Execute(Context) as String;
                return Evaluator.EvaluateStringEx(Actor, Actor, Actor,
                                Text, Context._databaseService, Context.OperationLimit);
            }
            else throw new RuntimeErrorException("Wrong number of arguments to EVAL.");
        }
    }
}
