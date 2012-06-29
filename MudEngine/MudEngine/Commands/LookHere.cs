using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class LookHere : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Target = Actor.Location.Parent;

            if (Target == null) _message.SendMessage(Actor, "You don't seem to be anywhere.\n");
            else
            {
                bool ShowDetail = MudCore.GetObjectRank(Actor) >= 5000 && Actor.HasAttribute("DISPLAYIDS");
                String Output = Target.GetAttribute("SHORT", Target.ID.ToString());
                if (ShowDetail) Output += "(" + Target.ID.ToString() + ")";
                Output += "\n";
                if (Target.HasAttribute("LONG")) Output +=
                                Evaluator.EvaluateAttribute(Actor, Target, Target, "LONG", "", _database) + "\n";

                String ExitList = "";
                for (int i = 0; i < (int)Cardinals.DirectionCount; ++i)
                {
                    String Name = Exits.ToString((Cardinals)i);
                    if (Target.HasAttribute(Name))
                    {
                        if (!String.IsNullOrEmpty(ExitList)) ExitList += ", ";
                        ExitList += Name;
                    }
                }

                if (!String.IsNullOrEmpty(ExitList)) Output += "Exits are " + ExitList + ".\n";

                Output += Evaluator.EvaluateString(Actor, Target, Target,
                    "<shortlist:me:IN:You also see \\list.\n>", _database);
                _message.SendMessage(Actor, Output);
            }
        }
    }
}
