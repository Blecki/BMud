using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Evaluate : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            //var _Actor = _match.GetArgument<Matchers.SearchSpaceObject>("ACTOR", null);
            //var Actor = _Actor.GetObject(_database);
            //var String = _match.GetArgument<String>("REST", null);

            //_message.SendMessage(Actor, Evaluator.EvaluateString(Actor, Actor, Actor, String, _database) + "\n");
        }
    }

    public class ShowParseTree : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            //var _Actor = _match.GetArgument<Matchers.SearchSpaceObject>("ACTOR", null);
            //var Actor = _Actor.GetObject(_database);
            //var _Of = _match.GetArgument<Matchers.SearchSpaceObject>("OBJECT", null);
            //var Of = _Of.GetObject(_database);
            //var Attribute = _match.GetArgument<String>("ATTRIBUTE", null);

            //_message.SendMessage(Actor, "Displaying parse tree for " + Attribute + " on " + _Of.Object.ID + "\n");

            //var Tree = Script.Parser.ParseScript(MudCore.QueryAttributeDefault(Of, Attribute, ""), 0);
            //if (Tree == null) _message.SendMessage(Actor, "null\n");
            //else Tree.Emit(Actor, _message, 0);
        }
    }
}
