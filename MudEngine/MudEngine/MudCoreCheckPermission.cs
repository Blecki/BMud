using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class MudCore
    {
        public static bool CheckPermission(MudObject Actor, MudObject Target, IDatabaseService Database)
        {
            if (Actor.ID == Target.ID) return true;
            int ActorRank = GetObjectRank(Actor);
            int TargetRank = GetObjectRank(Target);
            Int64 OwnerID = GetObjectOwner(Target);

            if (Actor.ID == OwnerID) return true;

            var TargetOwner = MudObject.FromID(OwnerID, Database);
            int OwnerRank = TargetOwner == null ? TargetRank : GetObjectRank(TargetOwner);

            if (ActorRank > 8000) return ActorRank > TargetRank; //At rank 8000 or higher, players can modify 
                                                                 //     objects owned by other wizards.
            else return ActorRank > OwnerRank; 
        }
    }
}
