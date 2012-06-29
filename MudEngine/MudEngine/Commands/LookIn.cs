using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class LookIn : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Attribute = _match.GetArgument<String>("LIST", null);
            var Target = _match.GetArgument<MudObject>("OBJECT", null);

            String PutLockName = "CANDROP" + Attribute.ToUpper();
            String LookLockName = "CANLOOK" + Attribute.ToUpper();

            if (!Script.Engine.PassesLock(LookLockName,
                (Target.HasAttribute(PutLockName) ? "" : "[You can't look " + Attribute.ToLower() + " that.\n]"),
                Actor, Target, Target, _database, _message)) return;

            var Room = Actor.Location.Parent;
            bool IsInInventory = Target.SearchUp(Actor);

            _message.SendMessage(Actor,
                Evaluator.EvaluateString(Actor, Target, Target,
                (!IsInInventory ?
                    "<shortlist:me:\\P:^\\P <me:the:the <me:short>> you see \\list:" +
                    "There doesn't seem to be anything \\P <me:the:the <me:short>>>.\n" :
                    "<shortlist:me:\\P:^\\P your <me:short> you see \\list:" +
                    "There doesn't seem to be anything \\P your <me:short>>.\n").Replace(
                    "\\P", Attribute.ToLower()), _database));

            MudCore.SendToContentsExceptActor(Room, Actor, Target, Target,
                        (!IsInInventory ? "<actor:short> looks \\P <me:the:the <me:short>>.\n" :
                        "<actor:short> looks \\P <me:possessive:his> <me:short>.\n").Replace("\\P", Attribute.ToLower()),
                        _database, _message);
        }
         
    }
}
