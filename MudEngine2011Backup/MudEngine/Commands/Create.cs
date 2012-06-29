using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Create : ICommandProcessor
    {
        public void Perform(PossibleMatch _match, IDatabaseService _database, IMessageService _message)
        {
            var _Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Words = _match.GetArgument<String>("REST", null);

            var Tokens = Words.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var NewID = MudCore.AllocateID(_database);
            var NewObject = MudObject.FromID(NewID, _database);

            NewObject.SetAttribute("OWNER", _Actor.ID.ToString());
            MudObject.MoveObject(NewObject, _Actor, "HELD", 1);

            String Adjectives = "";
            for (int i = 0; i < Tokens.Length - 1; ++i)
                Adjectives += Tokens[i].ToLower() + ":";

            String Noun = Tokens[Tokens.Length - 1].ToLower();

            String Short = "";
            for (int i = 0; i < Tokens.Length - 1; ++i)
                Short += Tokens[i].ToLower() + " ";
            Short += Noun;

            NewObject.SetAttribute("SHORT", Short);
            NewObject.SetAttribute("NOUNS", Noun);
            if (!String.IsNullOrEmpty(Adjectives)) NewObject.SetAttribute("ADJECTIVES", Adjectives);
            _message.SendMessage(_Actor, "Created new object with ID " + NewObject.ID.ToString() + "\n");

        }
    }

    public class Recycle : ICommandProcessor
    {
        public void Perform(PossibleMatch _match, IDatabaseService _database, IMessageService _message)
        {
            var _Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var _Object = _match.GetArgument<MudObject>("OBJECT", null);

            //var Attributes = _Object.GetAllAttributes();
            //foreach (var Attribute in Attributes)
            //    _database.RemoveAttribute(Object.ID, Attribute.Key);
            _Object.SetAttribute("SHORT", "DESTROYED");
            //_database.WriteAttribute(Object.ID, "SHORT", "DESTROYED");

            MudCore.RecycleID(_Object.ID, _database);
            _message.SendMessage(_Actor, "Recycled.\n");
        }
    }
}
