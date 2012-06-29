using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Wear : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var What = _match.GetArgument<MudObject>("OBJECT", null);

            if (!Script.Engine.PassesLock("CANWEAR", "[You can't war that.\n]", Actor, What, What, _database, _message))
                return;

            MudObject.MoveObject(What, Actor, "WORN", 1);

            MudCore.SendToContentsExceptActor(Actor.Location.Parent, Actor, What, What,
                "<actor:short> puts on <me:a:a <me:short>>.\n", _database, _message);
            _message.SendMessage(Actor, Evaluator.EvaluateString(Actor, What, What,
                "You put on <me:a:a <me:short>>.\n", _database));
        }
    }

    public class Remove : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var What = _match.GetArgument<MudObject>("OBJECT", null);

            if (!Script.Engine.PassesLock("CANWEAR", "[You can't war that.\n]", Actor, What, What, _database, _message))
                return;

            MudObject.MoveObject(What, Actor, "HELD", 1);

            MudCore.SendToContentsExceptActor(Actor.Location.Parent, Actor, What, What,
                "<actor:short> takes off <me:a:a <me:short>>.\n", _database, _message);
            _message.SendMessage(Actor, Evaluator.EvaluateString(Actor, What, What,
                "You take off <me:a:a <me:short>>.\n", _database));
        }
    }

}
