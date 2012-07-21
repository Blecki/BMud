﻿using System;
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

        public override void Execute(MudEngine2012.MudCore core)
        {
            try
            {
                bool displayMatches = false;
                bool displayTrace = false;
                bool time = false;

                if (_Command.StartsWith("/eval "))
                {
                    core.SendMessage(Executor,
                        MISP.ScriptObject.AsString(
                            core.scriptEngine.EvaluateString(new MISP.ScriptContext(), Executor,
                            _Command.Substring(6))), true);
                    core.SendPendingMessages();
                    return;
                }
                else if (_Command.StartsWith("/match "))
                    displayMatches = true;
                else if (_Command.StartsWith("/trace "))
                    displayTrace = true;
                else if (_Command.StartsWith("/time "))
                    time = true;

                var tokens = CommandTokenizer.FullyTokenizeCommand(_Command);
                var firstWord = tokens.word;
                tokens = tokens.next;

                if (displayMatches || displayTrace || time)
                {
                    firstWord = tokens.word;
                    tokens = tokens.next;
                }

                var matchContext = new MISP.ScriptContext();

                if (displayTrace)
                {
                    matchContext.trace = (s) => core.SendMessage(Executor, s, true);
                    matchContext.traceDepth = 0;
                }

                core.InvokeSystem(Executor, "handle_command",
                    new MISP.ScriptList(Executor, firstWord, _Command, tokens, displayMatches == true ? (object)true : null),
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