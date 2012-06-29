using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Go : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var _Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var RawExit = _match.GetArgument<String>("DIRECTION", null);


            Cardinals Cardinal = Exits.ToCardinal(RawExit);
            String ExitName = Exits.ToString(Cardinal); //Will convert abbreviations to full word.
            if (!_Actor.Location.Parent.HasAttribute(ExitName))
            {
                _message.SendMessage(_Actor, "You can't go that way.\n");
                return;
            }

            Int64 Destination = Exits.GetLinkTarget(_Actor.Location.Parent, Cardinal);
            var DestLocation = MudObject.FromID(Destination, _database);
            if (DestLocation == null)
            {
                _message.SendMessage(_Actor, "That exit appears to be broken. It shouldn't be.\n");
                return;
            }

            if (!Script.Engine.PassesLock("CANGO" + ExitName, "", _Actor, _Actor.Location.Parent, DestLocation,
                _database, _message)) return;

            if (!MudCore.CostStat(_Actor, "STAMINA", 1))
            {
                _message.SendMessage(_Actor, "You are too tired to do that.\n");
                return;
            }

            String DefaultMessageActor = "You go " + ExitName.ToLower() + ".\n";
            String DefaultLeaveMessage = "<actor:short> goes " + ExitName.ToLower() + ".\n";
            String DefaultArriveMessage = "<actor:short> arrives from the " + Exits.ToString(Exits.Opposite(Cardinal)).ToLower() + ".\n";

            var Room = _Actor.Location.Parent;
            _message.SendMessage(_Actor, Evaluator.EvaluateString(_Actor, Room, Room, DefaultMessageActor, _database));

            MudCore.SendToContentsExceptActor(Room, _Actor, _Actor, _Actor, DefaultLeaveMessage, _database, _message);
            MudCore.SendToContentsExceptActor(DestLocation, _Actor, _Actor, _Actor, DefaultArriveMessage, _database, _message);

            MudObject.MoveObject(_Actor, DestLocation, "IN", 1);
            _message.Command(_Actor.ID, "look");

            if (Room.HasAttribute("ONLEAVE"))
            {
                var Variables = new Script.VariableSet();
                Variables.Upsert("ACTOR", _Actor);
                Variables.Upsert("ME", Room);
                Script.Engine.ExecuteScript(Room, Room.GetAttribute("ONLEAVE", ""), Variables, _database, _message);
            }

            if (DestLocation.HasAttribute("ONENTER"))
            {
                var Variables = new Script.VariableSet();
                Variables.Upsert("ACTOR", _Actor);
                Variables.Upsert("ME", DestLocation);
                Script.Engine.ExecuteScript(Room, DestLocation.GetAttribute("ONENTER", ""), Variables, _database, _message);
            }
        }
    }
}
