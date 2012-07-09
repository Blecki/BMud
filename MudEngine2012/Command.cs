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

                if (_Command[0] == '/')
                {
                    core.SendMessage(Executor,
                        ScriptObject.AsString(
                            core.scriptEngine.EvaluateString(new ScriptContext(), Executor,
                            _Command.Substring(1))), true);
                    core.SendPendingMessages();
                    return;
                }

                var tokens = CommandTokenizer.FullyTokenizeCommand(_Command);
                var firstWord = tokens.word;
                tokens = tokens.next;

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
                        try
                        {
                            matchContext.Reset(Executor);
                            matchContext.PushVariable("command", _Command);
                            matchContext.PushVariable("actor", Executor);
                            matches = new ScriptList();
                            matches.Add(new GenericScriptObject("token", tokens, "actor", Executor));
                            arguments.Clear();
                            arguments.Add(matches);
                            matches = verb.Matcher.Invoke(matchContext, Executor, arguments) as ScriptList;
                        }
                        catch (ScriptError e)
                        {
                            core.SendMessage(Executor, e.Message, true);
                            matches = null;
                        }

                        if (matches == null || matches.Count == 0) continue;
                        if (!(matches[0] is GenericScriptObject))
                        {
                            core.SendMessage(Executor, "Matcher returned the wrong type.", true);
                            continue;
                        }

                        matchFound = true;
                        arguments.Clear();
                        arguments.Add(matches);
                        arguments.Add(Executor);
                        verb.Action.Invoke(matchContext, Executor, arguments);
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