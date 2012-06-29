using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Drop : ICommandProcessor
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
            var Count = _match.GetArgument<Int32>("COUNT", 1);

            if (What.Count < Count)
            {
                _message.SendMessage(Actor, "You don't have " + Count.ToVerbal() + " of those.\n");
                return;
            }

            if (Where == null)
            {
                Where = Actor.Location.Parent;
                List = "IN";
            }

            bool WhereIsRoom = (Where == Actor.Location.Parent);
            bool IsInInventory = (Where.FindTopObject() == Actor);

            String LockName = "CANDROP" + List.ToUpper();
            if (!Script.Engine.PassesLock(LockName, 
                (WhereIsRoom ? "" : "[You can't put things " + List.ToLower() + " that.\n]"),
                Actor, Where, What, _database, _message)) return;

            if (!Script.Engine.PassesLock("CANDROP", "", Actor, What, Where, _database, _message)) return;


                String Message = "<actor:short> ";

                if (WhereIsRoom) Message += "drops ";
                else Message += "puts ";

                if (Count == 1) Message += "<object:a:a <me:short>>";
                else Message += Count.ToVerbal() + " <object:plural:<me:short>s>";

                if (WhereIsRoom) Message += ".\n";
                else
                {
                    Message += " " + List.ToLower();
                    if (!IsInInventory) Message += " <me:the:the <me:short>>.\n";
                    else Message += " <actor:possessive:his> <me:short>.\n";
                }

            var Objects = Actor.Location.Parent.GetContents("IN");
            foreach (var Object in Objects)
                if (Object != Actor) _message.SendMessage(Object, Evaluator.EvaluateString(Actor, Where, What, Message, _database));

            String ActorMessage = "You ";

            if (WhereIsRoom) ActorMessage += "drop ";
            else ActorMessage += "put ";

            if (Count == 1) ActorMessage += "<object:a:a <me:short>>";
            else ActorMessage += Count.ToVerbal() + " <object:plural:<me:short>s>";

            if (WhereIsRoom) ActorMessage += ".\n";
            else
            {
                ActorMessage += " " + List.ToLower();
                if (!IsInInventory) ActorMessage += " <me:the:the <me:short>>.\n";
                else ActorMessage += " your <me:short>.\n";
            }

            _message.SendMessage(Actor, Evaluator.EvaluateString(Actor, Where, What, ActorMessage, _database));

            if (Where.Stacked)
                Where = Where.Location.Parent.UnStack(Where.Location.List, Where.Location.Index, 1);
            MudObject.MoveObject(What, Where, List, Count);

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

            String ScriptAttribute = "ONDROP" + List.ToUpper();
            if (Where.HasAttribute(ScriptAttribute))
            {
                var Variables = new Script.VariableSet();
                Variables.Upsert("ACTOR", Actor);
                Variables.Upsert("ME", Where);
                Variables.Upsert("OBJECT", What);
                Variables.Upsert("COUNT", new Script.Integer(Count));
                Script.Engine.ExecuteScript(Where, Where.GetAttribute(ScriptAttribute, ""), Variables, _database, _message);
            }


        }
    }
}
