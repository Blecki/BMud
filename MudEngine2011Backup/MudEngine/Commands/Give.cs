using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Give : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var What = _match.GetArgument<MudObject>("OBJECT", null);
            var Where = _match.GetArgument<MudObject>("TO", null);
            var Count = _match.GetArgument<Int32>("COUNT", 1);

            if (What.Count < Count)
            {
                _message.SendMessage(Actor, "You don't have " + Count.ToVerbal() + " of those.\n");
                return;
            }

            if (!Script.Engine.PassesLock("CANACCEPT",
                (Where.HasAttribute("PERSON") ? "" : "[You can't give things to that.\n]"),
                Actor, Where, What, _database, _message)) return;

            if (!Script.Engine.PassesLock("CANDROP", "", Actor, What, Where, _database, _message)) return;

            String Message = "<actor:short> gives ";
            if (Count == 1) Message += "<object:a:a <me:short>>";
            else Message += Count.ToVerbal() + " <object:plural:<me:short>s>";
            Message += " to <me:a:<me:short>>.\n";
            MudCore.SendToContentsExceptActor(Actor.Location.Parent, Actor, Where, What, Message, _database, _message);

            String ActorMessage = "You give ";
            if (Count == 1) ActorMessage += "<object:a:a <me:short>>";
            else ActorMessage += Count.ToVerbal() + " <object:plural:<me:short>s>";
            ActorMessage += " to <me:a:<me:short>>.\n";
            _message.SendMessage(Actor, Evaluator.EvaluateString(Actor, Where, What, ActorMessage, _database));

            MudObject.MoveObject(What, Where, "HELD", Count);

            //Execute script hooks
            if (What.HasAttribute("ONDROP"))
            {
                var Variables = new Script.VariableSet();
                Variables.Upsert("ACTOR", Actor);
                Variables.Upsert("ME", What);
                Variables.Upsert("OBJECT", Where);
                Variables.Upsert("COUNT", new Script.Integer(Count));
                Script.Engine.ExecuteScript(What, What.GetAttribute("ONDROP", ""), Variables, _database, _message);
            }

            if (Where.HasAttribute("ONGIVE"))
            {
                var Variables = new Script.VariableSet();
                Variables.Upsert("ACTOR", Actor);
                Variables.Upsert("ME", Where);
                Variables.Upsert("OBJECT", What);
                Variables.Upsert("COUNT", new Script.Integer(Count));
                Script.Engine.ExecuteScript(Where, Where.GetAttribute("ONGIVE", ""), Variables, _database, _message);
            }
        }
    }
}
