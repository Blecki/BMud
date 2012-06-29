using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Link : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match, 
            IDatabaseService _database, 
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            String Direction = _match.GetArgument<String>("DIRECTION", null);
            var Target = _match.GetArgument<MudObject>("OBJECT", null);

            var Room = Actor.Location.Parent;            
            if (!MudCore.CheckPermission(Actor, Room, _database))
                _message.SendMessage(Actor, "You do not have permission to modify this room.\n");
            else if (!MudCore.CheckPermission(Actor, Target, _database))
                _message.SendMessage(Actor, "You do not have permission to modify the room you are linking too.\n");
            else
            {
                Cardinals Cardinal = Exits.ToCardinal(Direction);
                if (Exits.GetLinkTarget(Room, Cardinal) != DatabaseConstants.Invalid)
                    Exits.DestroyLink(Room, Cardinal, _database);
                if (Exits.GetLinkTarget(Target, Exits.Opposite(Cardinal)) != DatabaseConstants.Invalid)
                    Exits.DestroyLink(Target, Exits.Opposite(Cardinal), _database);
                Exits.CreateLink(Room, Target, Cardinal); ;
                _message.SendMessage(Actor, "Created link.\n");
            }
        }
    }

    public class UnLink : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            String Direction = _match.GetArgument<String>("DIRECTION", null);

            var Room = Actor.Location.Parent;

            if (!MudCore.CheckPermission(Actor, Room, _database))
                _message.SendMessage(Actor, "You do not have permission to modify this room.\n");
            else
            {
                Cardinals Cardinal = Exits.ToCardinal(Direction);
                Exits.DestroyLink(Room, Cardinal, _database);
                _message.SendMessage(Actor, "Destroyed link.\n");
            }
        }
    }

    
}
