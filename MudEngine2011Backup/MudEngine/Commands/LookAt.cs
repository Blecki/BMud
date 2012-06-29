using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class LookAt : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Target = _match.GetArgument<MudObject>("OBJECT", null);
            bool ShowDetail = MudCore.GetObjectRank(Actor) >= 5000 && Actor.HasAttribute("DISPLAYIDS");
                
            String Output = "";
            Output += Evaluator.EvaluateString(Actor, Target, Target,
                "You see <me:a:a <me:short>>.", _database) + (ShowDetail ? ("(" + Target.ID.ToString() + ")") : "") + "\n";
            Output += Evaluator.EvaluateAttribute(Actor, Target, Target, "LONG", "You see nothing special.", _database) + "\n";

            if (Target.HasAttribute("person"))
            {
                Output += Evaluator.EvaluateString(Actor, Target, Target,
                    "<shortlist:FALSE:me:WORN:<me:short> is wearing \\list:<me:short> is naked>.\n", _database);
                Output += Evaluator.EvaluateString(Actor, Target, Target,
                    "<shortlist:FALSE:me:HELD:<me:short> is holding \\list:<me:short> is empty-handed>.\n", _database);
            }

            var OnItems = Target.GetContents("ON");
            if (OnItems.Count > 0)
                Output += Evaluator.EvaluateString(Actor, Target, Target,
                    "<shortlist:me:ON:On <me:the:the <me:short>> you see \\list: >.\n", _database);

            _message.SendMessage(Actor, Output);

            MudCore.SendToContentsExceptActor(Actor.Location.Parent, Actor, Target, Target,
                "<actor:short> looks at <me:a:a <me:short>>.\n", _database, _message);

        }
    }
}
