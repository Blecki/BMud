using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.ProtectedAttributes
{
    public class NextObjectID : IProtectedAttribute
    {

        public bool Check(MudObject Actor, MudObject Object, string NewValue, IMessageService Message)
        {
                Message.SendMessage(Actor, "You can't modify that attribute. EVER.\n");
                return false;
        }

        public bool CanDelete(MudObject Actor, MudObject Object, IMessageService Message)
        {
                Message.SendMessage(Actor, "Go ahead and delete that.. if you want to fuck over the entire database.\n");
                return false;
        }
    }
}
