using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class MudCore
    {
        public static void SendToContentsExceptActor(
            MudObject Room,
            MudObject Actor,
            MudObject Me,
            MudObject Object,
            String Message,
            IDatabaseService _database,
            IMessageService _message)
        {
            if (Room != null)
            {
                var Contents = Room.GetContents("IN");
                foreach (var _Object in Contents)
                    if (_Object != Actor) _message.SendMessage(_Object,
                        Evaluator.EvaluateString(Actor, Me, Object, Message, _database));
            }
        }

        public static void SendToList(
            List<MudObject> List,
            MudObject Actor,
            MudObject Me,
            MudObject Object,
            String Message,
            IDatabaseService _database,
            IMessageService _message)
        {
            foreach (var Item in List)
                _message.SendMessage(Item, Evaluator.EvaluateString(Actor, Me, Object, Message, _database));
        }
    }
}
