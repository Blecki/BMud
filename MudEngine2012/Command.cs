using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MudEngine2012
{
    internal class Command : PendingAction
    {
        internal MISP.ScriptObject Executor;
        internal String _Command;

        internal Command(MISP.ScriptObject Executor, String _Command)
            : base(0.1f)
        {
            this.Executor = Executor;
            this._Command = _Command;
        }

        public override void Execute(MudEngine2012.MudCore core)
        {
            try
            {
                bool displayMatches = false;
                bool displayTrace = false;
                bool time = false;
                var matchContext = new MISP.Context();

                var tokens = CommandTokenizer.FullyTokenizeCommand(_Command);
                String @switch = "";

                if (_Command.StartsWith("/"))
                {
                    @switch = tokens.word;
                    //if (core.InvokeSystemR(Executor, "allow-switch", new MISP.ScriptList(Executor, @switch), matchContext)
                    //    == null)
                    //{
                    //    core.SendMessage(Executor, "You don't have permission to use that switch.\n", true);
                    //    return;
                    //}

                    if (_Command.StartsWith("/eval "))
                    {
                        core.SendMessage(Executor,
                            MISP.ScriptObject.AsString(
                                core.scriptEngine.EvaluateString(new MISP.Context(), Executor,
                                _Command.Substring(6), "")), true);
                        core.SendPendingMessages();
                        return;
                    }
                    else if (_Command.StartsWith("/match "))
                        displayMatches = true;
                    else if (_Command.StartsWith("/trace "))
                        displayTrace = true;
                    else if (_Command.StartsWith("/time "))
                        time = true;

                    tokens = tokens.next;
                }


                if (displayTrace)
                {
                    matchContext.trace = (s) => core.SendMessage(Executor, s, true);
                    matchContext.traceDepth = 0;
                }

                core.InvokeSystem(Executor, "handle-client-command",
                    new MISP.ScriptList(Executor, _Command, tokens, @switch),
                    matchContext);

                if (time)
                {
                    var elapsed = DateTime.Now - matchContext.executionStart;
                    core.SendMessage(Executor, "[Command executed in " + elapsed.TotalMilliseconds + " milliseconds.]\n", true);
                }
            }
            catch (Exception e)
            {
                core.PendingMessages.Clear();
                core.SendMessage(Executor,
                    e.Message + "\n" +
                    e.StackTrace + "\n", true);
            }

        }
    }
}