using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class MudCore
    {
        public static Int64 CreateNewAccountPlayerObject(String Name, IDatabaseService Database)
        {
            var PlayerObject = Database.CreateObject();
            Database.WriteAttribute(PlayerObject, "short", Name);
            Database.WriteAttribute(PlayerObject, "person", "");
            Database.WriteAttribute(PlayerObject, "rank", "5000");
            Database.WriteAttribute(PlayerObject, "nouns", Name);
            Database.WriteAttribute(PlayerObject, "a", Name);
            Database.WriteAttribute(PlayerObject, "staminamax", "40");
            return PlayerObject;
        }
    }
}
