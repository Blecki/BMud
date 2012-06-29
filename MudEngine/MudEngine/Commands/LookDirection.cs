using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{   
    public class LookDirection : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var RawExit = _match.GetArgument<String>("DIRECTION", null);

            var Cardinal = Exits.ToCardinal(RawExit); //Will convert abbreviations
            String ExitName = Exits.ToString(Cardinal); //to the full word.

            if (!Actor.Location.Parent.HasAttribute(ExitName))
                _message.SendMessage(Actor, "You don't see anything that way.\n");
            else
            {
                var Destination = MudObject.FromID(Exits.GetLinkTarget(Actor.Location.Parent, Cardinal), _database);
                if (!Destination.Valid)
                    _message.SendMessage(Actor, "That exit appears to be broken.\n");
                else
                {
                    bool ShowDetail = MudCore.GetObjectRank(Actor) >= 5000 && Actor.HasAttribute("DISPLAYIDS");
                    String Output = "";
                    if (Cardinal == Cardinals.Up) Output = "Above";
                    else if (Cardinal == Cardinals.Down) Output = "Below";
                    else Output = "To the " + Exits.ToString(Cardinal).ToLower();
                    Output += " you see " + Destination.GetAttribute("short", Destination.ID.ToString());
                    if (ShowDetail) Output += "(" + Destination.ID.ToString() + ")";
                    Output += "\n";

                    Output += Evaluator.EvaluateString(Actor, Destination, Destination,
                        "<shortlist:me:IN:Also there \\is \\list.\n>", _database);
                    _message.SendMessage(Actor, Output);

                    MudCore.SendToContentsExceptActor(Actor.Location.Parent, Actor, Destination, Destination,
                        "<actor:short> looks " + ExitName.ToLower() + ".\n", _database, _message);
                }
            }
        }
    }
}
