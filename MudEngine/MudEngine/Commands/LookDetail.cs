using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class DetailMatcher : ICommandTokenMatcher
    {
        public List<PossibleMatch> Match(
            PossibleMatch _match,
            IDatabaseService _database,
            string _rawCommand)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);

            var Result = new List<PossibleMatch>();

            String DetailName = "";
            var Token = _match._next;
            while (Token != null)
            {
                DetailName += "_" + Token.Value.Value;
                Token = Token.Next;
            }

            if (Actor.Location.Parent.HasAttribute("DETAIL" + DetailName))
                Result.Add(new PossibleMatch(_match._arguments, null, "DETAIL", DetailName));
            return Result;
        }
    }

    public class LookDetail : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Target = _match.GetArgument<String>("DETAIL", null);

            String DetailName = "DETAIL" + Target;

            _message.SendMessage(Actor, Evaluator.EvaluateAttribute(
                Actor, Actor.Location.Parent, Actor.Location.Parent, DetailName, "", _database));
        }
    }
}
