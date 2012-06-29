using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Script
{
    internal class STARTTIMER : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count != 3) throw new RuntimeErrorException("STARTTIMER takes 3 arguments.");
            var Who = Children[0].Execute(Context) as MudObject;
            var When = Children[1].Execute(Context) as Integer;
            var Text = Children[2].Execute(Context) as String;

            if (!MudCore.CheckPermission(Context.Executor, Who, Context._databaseService)) return null;
            if (When.Value < 1 || When.Value > DatabaseConstants.TicksPerDay) return null;
            var Timers = Context._databaseService.QueryObjectTimers(Who.ID);
            if (Timers.Count((A) => { return A.Attribute == Text; }) == 0)
                Context._databaseService.StartTimer(Context._messageService.GetFutureTime(When.Value), Who.ID, Text);
            return Parser.True;
        }
    }

    internal class STOPTIMERS : FunctionCall
    {
        internal override Object Execute(ExecutionContext Context)
        {
            Context.OperationLimit.DecThrow();

            if (Children.Count != 1) throw new RuntimeErrorException("STOPTIMERS takes 1 argument1.");
            var Who = Children[0].Execute(Context) as MudObject;

            if (!MudCore.CheckPermission(Context.Executor, Who, Context._databaseService)) return null;
            Context._databaseService.StopTimers(Who.ID);
            return Parser.True;
        }
    }
}
