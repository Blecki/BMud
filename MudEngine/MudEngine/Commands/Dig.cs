using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class DigDirection : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match, 
            IDatabaseService _database, 
            IMessageService _message)
        {
            var _Actor = _match.GetArgument<MudObject>("ACTOR", null);
            String Direction = _match.GetArgument<String>("DIRECTION", null);
            String Short = _match.GetArgument<String>("REST", null);

            if (!MudCore.CheckPermission(_Actor, _Actor.Location.Parent, _database))
                _message.SendMessage(_Actor, "You do not have permission to modify this room.\n");
            else
            {
                Cardinals Cardinal = Exits.ToCardinal(Direction);
                if (_Actor.Location.Parent.HasAttribute(Exits.ToString(Cardinal)))
                {
                    _message.SendMessage(_Actor, "There is already something to the " + Exits.ToString(Cardinal) + ".\n");
                    return;
                }

                Int64 NewRoomID = MudCore.AllocateID(_database);
                var NewRoom = MudObject.FromID(NewRoomID, _database);
                NewRoom.SetAttribute("SHORT", Short);
                NewRoom.SetAttribute("OWNER", _Actor.ID.ToString());
                Exits.CreateLink(_Actor.Location.Parent, NewRoom, Cardinal);
                _message.SendMessage(_Actor, "Created and linked new room : " + Short + "\n");
                _message.Command(_Actor.ID, "GO " + Direction);
            }
        }
    }

    public class Dig : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var _Actor = _match.GetArgument<MudObject>("ACTOR", null);
            String Short = _match.GetArgument<String>("REST", null);

            var NewRoom = MudObject.FromID(MudCore.AllocateID(_database), _database);
            NewRoom.SetAttribute("SHORT", Short);
            NewRoom.SetAttribute("OWNER", _Actor.ID.ToString());

            _message.SendMessage(_Actor, "Created new room : " + Short + "\n");
            _message.Command(_Actor.ID, "TELEPORT ME #" + NewRoom.ID.ToString() + " IN");
            _message.Command(_Actor.ID, "LOOK");
        }
    }
}
