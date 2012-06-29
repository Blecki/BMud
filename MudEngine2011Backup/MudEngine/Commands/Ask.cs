using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Commands
{
    public class Ask : ICommandProcessor
    {
        public static String Trim(String _str)
        {
            String R = "";
            foreach (var c in _str.ToUpper())
                if (c >= 'A' && c <= 'Z') R += c;
            return R;
        }

        public void Perform(
            PossibleMatch _match,
            IDatabaseService _database,
            IMessageService _message)
        {
            var Actor = _match.GetArgument<MudObject>("ACTOR", null);
            var Who = _match.GetArgument<MudObject>("WHO", null);
            var Text = _match.GetArgument<String>("TEXT", null);

            Text = Trim(Text);

            if (String.IsNullOrEmpty(Text))
            {
                _message.SendMessage(Actor, "Ask them about what?\n");
                return;
            }

            MudCore.SendToContentsExceptActor(Actor.Location.Parent, Actor, Who, Who,
                "<actor:short> asks <me:short> about " + Text.ToLower() + ".\n", _database, _message);

            if (!Who.HasAttribute("NPC"))
            {
                _message.SendMessage(Actor, "There is no response.\n");
                return;
            }

            String TopicAttribute = "TOPIC_" + Text.ToUpper();

            if (Who.HasAttribute(TopicAttribute))
            {
                _message.Command(Who.ID, "SAY " +
                    Evaluator.EvaluateString(Actor, Who, Who, Who.GetAttribute(TopicAttribute, ""), _database));
            }
            else
            {
                _message.SendMessage(Actor,
                    Evaluator.EvaluateString(Actor, Who, Who, Who.GetAttribute("NPC", ""), _database));
            }

        }
    }
    
}
