using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Drink : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var What = _match.GetArgument<MudObject>("OBJECT", null);

            if (!Script.Engine.PassesLock("CANDRINK", "[You can't drink that.\n]", Actor, What, What, _database, _message))
                return;

            MudCore.SendToContentsExceptActor(Actor.Location.Parent, Actor, What, What,
                "<actor:short> drinks <me:a:a <me:short>>.\n", _database, _message);

            _message.SendMessage(Actor, Evaluator.EvaluateString(Actor, What, What, "You drink <me:a:a <me:short>>.\n", _database));

            if (What.HasAttribute("ONDRINK"))
            {
                var Variables = new Script.VariableSet();
                Variables.Upsert("ACTOR", Actor);
                Variables.Upsert("ME", What);
                Script.Engine.ExecuteScript(What, What.GetAttribute("ONDRINK", ""), Variables, _database, _message);
            }
        }
    }

    public class Eat : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var What = _match.GetArgument<MudObject>("OBJECT", null);

            if (!Script.Engine.PassesLock("CANEAT", "[You can't eat that.\n]", Actor, What, What, _database, _message))
                return;

            MudCore.SendToContentsExceptActor(Actor.Location.Parent, Actor, What, What,
                "<actor:short> eats <me:a:a <me:short>>.\n", _database, _message);

            _message.SendMessage(Actor, Evaluator.EvaluateString(Actor, What, What, "You eat <me:a:a <me:short>>.\n", _database));

            if (What.HasAttribute("ONEAT"))
            {
                var Variables = new Script.VariableSet();
                Variables.Upsert("ACTOR", Actor);
                Variables.Upsert("ME", What);
                Script.Engine.ExecuteScript(What, What.GetAttribute("ONEAT", ""), Variables, _database, _message);
            }
        }
    }
}
