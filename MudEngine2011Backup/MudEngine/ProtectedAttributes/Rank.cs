using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.ProtectedAttributes
{
    public class Rank : IProtectedAttribute
    {
        public bool Check(MudObject Actor, MudObject Object, string NewValue, IMessageService Message)
        {
            try
            {
                Int32 Rank = Convert.ToInt32(NewValue);
                Int32 ActorRank = Convert.ToInt32(Actor.GetAttribute("RANK", "0"));
                if (Rank >= ActorRank)
                {
                    Message.SendMessage(Actor, "You can't promote objects to or higher than your own rank.\n");
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                Message.SendMessage(Actor, "The value of the RANK attribute must be an integer.\n");
                return false;
            }
        }

        public bool CanDelete(MudObject Actor, MudObject Object, IMessageService Message)
        {
            return true;
        }
    }
}
