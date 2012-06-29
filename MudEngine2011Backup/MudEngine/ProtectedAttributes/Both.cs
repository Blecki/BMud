using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.ProtectedAttributes
{
    public class Both : IProtectedAttribute
    {
        IProtectedAttribute First, Second;

        public Both(IProtectedAttribute First, IProtectedAttribute Second)
        {
            this.First = First;
            this.Second = Second;
        }

        public bool Check(MudObject Actor, MudObject Object, string NewValue, IMessageService Message)
        {
            return First.Check(Actor, Object, NewValue, Message) && Second.Check(Actor, Object, NewValue, Message);
        }

        public bool CanDelete(MudObject Actor, MudObject Object, IMessageService Message)
        {
            return First.CanDelete(Actor, Object, Message) && Second.CanDelete(Actor, Object, Message);
        }
    }
}
