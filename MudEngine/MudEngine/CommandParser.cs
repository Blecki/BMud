using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class CommandParser
    {
        internal class CommandEntry
        {
            internal ICommandTokenMatcher RootMatcher;
            internal ICommandProcessor Processor;

            internal List<PossibleMatch> Match(
                String _command, 
                MudObject _actor, 
                IDatabaseService _database)
            {
                var Tokens = FullyTokenizeCommand(_command);

                var NextToken = Tokens.First;
                while (NextToken != null)
                {
                    var KeepToken = NextToken.Next;
                    if (NextToken.Value.Value.ToUpper() == "THE")
                        Tokens.Remove(NextToken);
                    NextToken = KeepToken;
                }

                var Result = new PossibleMatch(Tokens.First, "ACTOR", _actor);
                return RootMatcher.Match(Result, _database, _command);
            }
        }

        internal class CommandSet : List<CommandEntry>
        {
            internal int MinimumRank = 0;
        }

        internal Dictionary<String, CommandSet> Commands = new Dictionary<String, CommandSet>();
        internal Dictionary<String, String> Alias = new Dictionary<String, String>();
        internal Dictionary<String, String> HelpTopics = new Dictionary<String, String>();
        internal CommandSet DefaultCommandSet = new CommandSet { MinimumRank = 0 };

        public void AddCommandSet(String KeyWord, ICommandTokenMatcher Matcher, ICommandProcessor Processor)
        {
            var Entry = new CommandEntry { RootMatcher = Matcher, Processor = Processor };
            if (Commands.ContainsKey(KeyWord.ToUpper())) Commands[KeyWord.ToUpper()].Add(Entry);
            else
            {
                Commands.Add(KeyWord.ToUpper(), new CommandSet());
                Commands[KeyWord.ToUpper()].Add(Entry);
            }
        }

        public void AddDefaultCommand(ICommandTokenMatcher Matcher, ICommandProcessor Processor)
        {
            DefaultCommandSet.Add(new CommandEntry { RootMatcher = Matcher, Processor = Processor });
        }

        public void SetCommandRank(String KeyWord, int Rank)
        {
            if (Commands.ContainsKey(KeyWord.ToUpper())) Commands[KeyWord.ToUpper()].MinimumRank = Rank;
        }

        public void AddCommandAlias(String Alias, String Command)
        {
            this.Alias.Add(Alias.ToUpper(), Command.ToUpper());
        }

        public void AddHelpTopic(String Topic, String Text)
        {
            HelpTopics.Add(Topic.ToUpper(), Text);
        }

        public class ProcessState
        {
            public PossibleMatch Match;
            public ICommandProcessor Processor;
        }

        private class HelpImplementation : ICommandProcessor
        {
            internal String Text;

            public void Perform(PossibleMatch _match, IDatabaseService _database, IMessageService _message)
            {
                _message.SendMessage(_match.GetArgument<MudObject>("ACTOR", null), Text);
            }
        }

        private HelpImplementation _helpCommand = new HelpImplementation();            

        internal ProcessState ParseCommand(
            String _command,
            MudObject _actor,
            IDatabaseService _database)
        {
            String firstWord, remainder;
            TokenizeCommand(_command, out firstWord, out remainder);
            firstWord = firstWord.ToUpper();

            if (firstWord == "HELP" || firstWord == "?")
            {
                if (String.IsNullOrEmpty(remainder))
                {
                    _helpCommand.Text = "";
                    foreach (var Pair in HelpTopics)
                        _helpCommand.Text += Pair.Key + ", ";
                    _helpCommand.Text += "\n";
                }
                else
                {
                    if (HelpTopics.ContainsKey(remainder.ToUpper()))
                        _helpCommand.Text = HelpTopics[remainder.ToUpper()];
                    else
                        _helpCommand.Text = "There doesn't seem to be any help on that topic.\n";
                }

                return new ProcessState
                {
                    Match = new PossibleMatch(null, "ACTOR", _actor),
                    Processor = _helpCommand
                };
            }
            else
            {
                CommandSet Set = null;
                if (Commands.ContainsKey(firstWord)) Set = Commands[firstWord];
                else if (Alias.ContainsKey(firstWord) && Commands.ContainsKey(Alias[firstWord])) Set = Commands[Alias[firstWord]];
                else
                {
                    Set = DefaultCommandSet;
                    remainder = _command;
                }
                if (Set == null) return null;
                if (Set.MinimumRank > MudCore.GetObjectRank(_actor)) return null;
                foreach (var Command in Set)
                {
                    var MatchSet = Command.Match(remainder, _actor, _database);
                    var FirstGoodMatch = MatchSet.Find((A) => { return A._next == null; });
                    if (FirstGoodMatch != null)
                        return new ProcessState { Match = FirstGoodMatch, Processor = Command.Processor };
                }
            }
            return null;
        }
        
    }
}
