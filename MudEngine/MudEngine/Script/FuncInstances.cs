using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class BANISH : FunctionCall
    {
        internal override object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            int Count = 1;
            MudObject What = null;

            if (Children.Count == 1)
                What = Children[0].Execute(Context) as MudObject;
            else if (Children.Count == 2)
            {
                Count = (Children[0].Execute(Context) as Integer).Value;
                What = Children[1].Execute(Context) as MudObject;
            }
            else throw new RuntimeErrorException("BANISH takes one or two arguments.");

            What.Banish(Count);
            return What;
        }
    }

    internal class INSTANCE : FunctionCall
    {
        internal override object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count != 4) throw new RuntimeErrorException("INSTANCE takes exactly four arguments.");

            var Count = Children[0].Execute(Context) as Integer;
            var What = Children[1].Execute(Context) as MudObject;
            var Where = Children[2].Execute(Context) as MudObject;
            var List = Children[3].Execute(Context) as String;

            var NewInstance = What.Instanciate(Count.Value);
            Where.AddChild(NewInstance, List);
            return NewInstance;
        }
    }

    internal class ISINSTANCE : FunctionCall
    {
        internal override object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();
            if (Children.Count != 1) throw new RuntimeErrorException("ISINSTANCE takes exactly one argument.");

            var What = Children[0].Execute(Context) as MudObject;
            if (What.Instance) return Parser.True;
            else return null;
        }
    }

    internal class DECOR : FunctionCall
    {
        internal override object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count != 3) throw new RuntimeErrorException("DECOR takes exactly three arguments.");

            var Where = Children[0].Execute(Context) as MudObject;
            var List = Children[1].Execute(Context) as String;
            var Parameters = Children[2].Execute(Context) as String;

            String A, B;
            var NewInstance = MudObject.FromID(-1, Context._databaseService).Instanciate(1);
            Commands.Decor.ImplementDecor(Parameters, NewInstance, out A, out B);
            Where.AddChild(NewInstance, List);
            return NewInstance;
        }
    }
}
