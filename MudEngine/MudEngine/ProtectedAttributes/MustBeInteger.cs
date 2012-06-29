using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.ProtectedAttributes
{
    public class MustBeInteger : IProtectedAttribute
    {
        public bool Check(MudObject Actor, MudObject Object, string NewValue, IMessageService Message)
        {
            try
            {
                Int64 Integer = Convert.ToInt64(NewValue);
                return true;
            }
            catch (Exception)
            {
                Message.SendMessage(Actor, "The value of that attribute must be an integer.\n");
                return false;
            }
        }

        public bool CanDelete(MudObject Actor, MudObject Object, IMessageService Message)
        {
            return true;
        }
    }
}
