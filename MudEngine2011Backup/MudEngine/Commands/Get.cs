using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Get : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var _Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var _What = _match.GetArgument<MudObject>("OBJECT", null);
            var Count = _match.GetArgument<Int32>("COUNT", 1);

            var Where = _What.Location.Parent;
            bool WhereIsRoom = (_Actor.Location.Parent == _What.Location.Parent);
            bool IsInInventory = (_What.FindTopObject() == _Actor);

            if (_What.Count < Count)
            {
                _message.SendMessage(_Actor, "There aren't " + Count.ToVerbal() + " of those " + (WhereIsRoom ? "here.\n" : "there.\n"));
                return;
            }

            if (!Script.Engine.PassesLock("CANGET", "", _Actor, _What, _What.Location.Parent, _database, _message)) return;
            if (!Script.Engine.PassesLock("CANGETFROM" + _What.Location.List, "", _Actor, _What.Location.Parent, 
                _What, _database, _message)) return;
            //if (!MudCore.CanHold(_Actor, _What, _database, _message)) return;

            String Message = "<actor:short> gets ";
            if (Count == 1) Message += "<me:a:a <me:short>>";
            else Message += Count.ToVerbal() + " <me:plural:<me:short>s>";

            if (WhereIsRoom)
                Message += ".\n";
            else
            {
                Message += " from " + _What.Location.List.ToLower();
                if (!IsInInventory)
                    Message += " <object:the:the <me:short>>.\n";
                else
                    Message += " <actor:possessive:his> <object:short>.\n";
            }

            var LocationContents = _Actor.Location.Parent.GetContents("IN");
            foreach (var Object in LocationContents)
                if (Object != _Actor) _message.SendMessage(Object,
                    Evaluator.EvaluateString(_Actor, _What, Where, Message, _database));

            String ActorMessage = "You get ";
            if (Count == 1) ActorMessage += "<me:a:a <me:short>>";
            else ActorMessage += Count.ToVerbal() + " <me:plural:<me:short>s>";

            if (WhereIsRoom)
                ActorMessage += ".\n";
            else
            {
                ActorMessage += " from " + _What.Location.List.ToLower();
                if (!IsInInventory)
                    ActorMessage += " <object:the:the <me:short>>.\n";
                else
                    ActorMessage += " your <object:short>.\n";
            }

            _message.SendMessage(_Actor, Evaluator.EvaluateString(_Actor, _What, Where, ActorMessage, _database));

            MudObject.MoveObject(_What, _Actor, "HELD", Count);

            //Execute script hooks
            if (_What.HasAttribute("ONGET"))
            {
                var Variables = new Script.VariableSet();
                Variables.Upsert("ACTOR", _Actor);
                Variables.Upsert("ME", _What);
                Variables.Upsert("OBJECT", _What.Location.Parent);
                Variables.Upsert("COUNT", new Script.Integer(Count));
                Script.Engine.ExecuteScript(_What, _What.GetAttribute("ONDROP", ""), Variables, _database, _message);
            }

            String ScriptAttribute = "ONGETFROM" + _What.Location.List.ToUpper();
            if (Where.HasAttribute(ScriptAttribute))
            {
                var Variables = new Script.VariableSet();
                Variables.Upsert("ACTOR", _Actor);
                Variables.Upsert("ME", _What.Location.Parent);
                Variables.Upsert("OBJECT", _What);
                Variables.Upsert("COUNT", new Script.Integer(Count));
                Script.Engine.ExecuteScript(Where, Where.GetAttribute(ScriptAttribute, ""), Variables, _database, _message);
            }
        }
    }
}
