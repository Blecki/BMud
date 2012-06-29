using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Say : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match, 
            IDatabaseService _database, 
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Text = _match.GetArgument<String>("REST", null);

            var Room = Actor.Location.Parent;

            MudCore.SendToContentsExceptActor(Room, Actor, Actor, Actor, "<actor:short> says \"" + Text + "\"\n",
                _database, _message);

            _message.SendMessage(Actor, "You say \"" + Text + "\"\n");
        }
    }

     public class Emote : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match, 
            IDatabaseService _database, 
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Text = _match.GetArgument<String>("REST", null);

            var Room = Actor.Location.Parent;

            MudCore.SendToContentsExceptActor(Room, Actor, Actor, Actor, "<actor:short> " + Text + "\n",
                _database, _message);
           
            _message.SendMessage(Actor, "You " + Text + "\n");
        }
    }
}
