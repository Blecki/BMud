using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.ProtectedAttributes
{
    public class MustBeRank : IProtectedAttribute
    {
        Int32 _rank;

        public MustBeRank(Int32 _rank) { this._rank = _rank; }

        public bool Check(MudObject Actor, MudObject Object, string NewValue, IMessageService Message)
        {
            Int32 ActorRank = MudCore.GetObjectRank(Actor);
            if (MudCore.GetObjectRank(Actor) < _rank)
            {
                Message.SendMessage(Actor, "You must be at least rank " + _rank.ToString() + " to modify that attribute.\n");
                return false;
            }
            return true;
        }

        public bool CanDelete(MudObject Actor, MudObject Object, IMessageService Message)
        {
            return MudCore.GetObjectRank(Actor) >= _rank;
        }
    }
}
