using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class LookMe : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);

            bool ShowDetail = MudCore.GetObjectRank(Actor) >= 5000 && Actor.HasAttribute("DISPLAYIDS");

            String Output = "";
            Output += Evaluator.EvaluateString(Actor, Actor, Actor,
                "<shortlist:me:WORN:You are wearing \\list:You are naked>.\n", _database);
            Output += Evaluator.EvaluateString(Actor, Actor, Actor,
                "<shortlist:me:HELD:You are holding \\list:You are empty-handed>.\n", _database);

            _message.SendMessage(Actor, Output);
        }
    }
}
