using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MudEngine2012
{
    internal class Command : PendingAction
    {
        internal MudObject Executor;
        internal String _Command;

        public override void Execute(MudEngine2012.MudCore core)
        {
            try
            {
                bool displayMatches = false;
                bool displayTrace = false;

                if (_Command.StartsWith("/eval "))
                {
                    core.SendMessage(Executor,
                        ScriptObject.AsString(
                            core.scriptEngine.EvaluateString(new ScriptContext(), Executor,
                            _Command.Substring(6))), true);
                    core.SendPendingMessages();
                    return;
                }
                else if (_Command.StartsWith("/match "))
                    displayMatches = true;
                else if (_Command.StartsWith("/trace "))
                    displayTrace = true;

                var tokens = CommandTokenizer.FullyTokenizeCommand(_Command);
                var firstWord = tokens.word;
                tokens = tokens.next;

                if (displayMatches || displayTrace)
                {
                    firstWord = tokens.word;
                    tokens = tokens.next;
                }

                var arguments = new ScriptList();
                var matchContext = new ScriptContext();
                ScriptList matches = null;

                List<Verb> verbList = null;
                if (core.verbs.ContainsKey(firstWord)) verbList = core.verbs[firstWord];
                else if (core.aliases.ContainsKey(firstWord) && core.verbs.ContainsKey(core.aliases[firstWord]))
                    verbList = core.verbs[core.aliases[firstWord]];

                if (verbList != null)
                {
                    bool matchFound = false;
                    foreach (var verb in verbList)
                    {
                        if (displayMatches || displayTrace) core.SendMessage(Executor, "Attempting to match " + verb.comment + "\n", true);
                        try
                        {
                            if (displayTrace)
                            {
                                matchContext.trace = (s) => core.SendMessage(Executor, s, true);
                                matchContext.traceDepth = 0;
                            }

                            matchContext.Reset(Executor);
                            matches = new ScriptList();
                            matches.Add(new GenericScriptObject("token", tokens, "actor", Executor, "command", _Command));
                            arguments.Clear();
                            arguments.Add(matches);
                            matches = verb.Matcher.Invoke(matchContext, Executor, arguments) as ScriptList;
                        }
                        catch (ScriptError e)
                        {
                            core.SendMessage(Executor, e.Message, true);
                            matches = null;
                        }

                        if (matches == null || matches.Count == 0)
                        {
                            if (displayMatches) core.SendMessage(Executor, "No matches.\n", true);
                            continue;
                        }

                        if (!(matches[0] is GenericScriptObject))
                        {
                            core.SendMessage(Executor, "Matcher returned the wrong type.", true);
                            continue;
                        }

                        matchFound = true;

                        if (displayMatches)
                        {
                            core.SendMessage(Executor, matches.Count.ToString() + " successful matches.\n", true);
                            foreach (var match in matches)
                                core.SendMessage(Executor, ScriptObject.AsString(match, 1) + "\n", true);
                        }
                        else
                        {
                            arguments.Clear();
                            arguments.Add(matches);
                            arguments.Add(Executor);
                            verb.Action.Invoke(matchContext, Executor, arguments);
                        }
                        break;
                    }

                    if (!matchFound)
                        core.SendMessage(Executor, "No registered matchers matched.", false);
                }
                else
                {
                    arguments.Clear();
                    arguments.Add(_Command);
                    arguments.Add(Executor);
                    if (!core.InvokeSystem(Executor, "on_unknown_verb", arguments, matchContext))
                        core.SendMessage(Executor, "I don't recognize that verb.", false);
                }
            }
            catch (Exception e)
            {
                core.PendingMessages.Clear();
                //DatabaseService.DiscardChanges();
                core.SendMessage(Executor,
                    e.Message + "\n" +
                    e.StackTrace + "\n", true);
            }

        }
    }
}