using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Instance : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var What = _match.GetArgument<MudObject>("OBJECT", null);
            var Count = _match.GetArgument<Int32>("COUNT", 1);

            var NewInstance = What.Instanciate(Count);
            Actor.AddChild(NewInstance, "HELD");
            _message.SendMessage(Actor, "You are now holding "
                + Count.ToString() + (Count > 1 ? " instances" : " instance") + " of " 
                + What.GetAttribute("SHORT", What.ID.ToString()) + ".\n");
        }
    }

    public class Decor : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Rest = _match.GetArgument<String>("REST", null);

            var NewInstance = MudObject.FromID(-1, _database).Instanciate(1);

            String A;
            String Short;
            ImplementDecor(Rest, NewInstance, out A, out Short);

            Actor.AddChild(NewInstance, "HELD");
            _message.SendMessage(Actor, "You are now holding " + A + Short + ".\n");
        }

        public static void ImplementDecor(string Rest, MudObject NewInstance, out String A, out String Short)
        {
            var Tokens = new List<String>(Rest.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            A = "";
            if (Tokens[0] == "a" || Tokens[0] == "an")
            {
                A += Tokens[0] + " ";
                Tokens.RemoveAt(0);
            }

            if (Tokens.Count == 0) throw new InvalidOperationException("Not enough parameters for decor.");

            String Adjectives = "";
            String Nouns = "";
            Short = "";

            for (int i = 0; i < Tokens.Count; ++i)
            {
                if (i < Tokens.Count - 1)
                    Adjectives += Tokens[i] + ":";
                else
                    Nouns += Tokens[i] + ":" + Tokens[i] + "s";
                if (!String.IsNullOrEmpty(Short)) Short += " ";
                Short += Tokens[i];
            }

            NewInstance.SetLocalAttribute("SHORT", Short);
            NewInstance.SetLocalAttribute("A", A + "<me:short>");
            NewInstance.SetLocalAttribute("NOUNS", Nouns);
            if (!String.IsNullOrEmpty(Adjectives)) NewInstance.SetLocalAttribute("ADJECTIVES", Adjectives);
        }
    }

    public class Banish : ICommandProcessor
    {
        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var What = _match.GetArgument<MudObject>("OBJECT", null);
            var Count = _match.GetArgument<Int32>("COUNT", 1);

            Actor.RemoveChild(What.Location.List, What.Location.Index, Count);
            _message.SendMessage(Actor, "Banished " + Count.ToString() + 
                (Count > 1 ? " instances" : " instance") + " of "
                + What.GetAttribute("SHORT", What.ID.ToString()) + ".\n");
        }
    }
}
