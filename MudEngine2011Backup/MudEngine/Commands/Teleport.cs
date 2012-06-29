using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Teleport : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var What = _match.GetArgument<MudObject>("OBJECT", null);
            var Where = _match.GetArgument<MudObject>("WHERE", null);
            var List = _match.GetArgument<String>("LIST", null);

            if (!MudCore.CheckPermission(Actor, What, _database))
                _message.SendMessage(Actor, "You don't have permission to modify that object.\n");
            else
            {
                MudObject.MoveObject(What, Where, List, 1);
                _message.SendMessage(Actor, "Teleported.\n");
            }
        }
    }
}
