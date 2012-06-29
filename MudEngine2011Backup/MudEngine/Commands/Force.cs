using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Force : ICommandProcessor
    {
        public void Perform(PossibleMatch _match, IDatabaseService _database, IMessageService _message)
        {
            var _Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Target = _match.GetArgument<MudObject>("OBJECT", null);
            var String = _match.GetArgument<String>("REST", null);

            if (!MudCore.CheckPermission(_Actor, Target, _database))
                _message.SendMessage(_Actor, "You can't force that.\n");
            else
                _message.Command(Target.ID, String);
        }
    }
}
