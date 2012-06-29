using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.ProtectedAttributes
{
    public class NeverEmpty : IProtectedAttribute
    {
        public bool Check(MudObject Actor, MudObject Object, string NewValue, IMessageService Message)
        {
            if (String.IsNullOrEmpty(NewValue))
            {
                Message.SendMessage(Actor, "This attribute cannot be empty.\n");
                return false;
            }
            return true;
        }

        public bool CanDelete(MudObject Actor, MudObject Object, IMessageService Message)
        {
            Message.SendMessage(Actor, "This attribute cannot be removed.\n");
            return false;
        }
    }
}
