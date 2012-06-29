using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class MudCore
    {
        public static void CreateDefaultDatabase(IDatabaseService Database)
        {
            var God = Database.CreateObject();
            Database.WriteAttribute(God, "short", "God");
            Database.WriteAttribute(God, "person", "");
            Database.WriteAttribute(God, "rank", "10000");
            Database.WriteAttribute(God, "location", "1:IN");
            Database.WriteAttribute(God, "nouns", "god");
            
            var StartRoom = Database.CreateObject();
            Database.WriteAttribute(StartRoom, "short", "Start Room");
            Database.WriteAttribute(StartRoom, "long", "This is the default database. Nothing is persisted yet, so don't worry about messing things up. For now, all new players are given a rank of 5000, giving them access to the build commands. To get started, enter the command 'help'. Select a topic, and enter the command 'help [topic]'. I suggest reading all the topics.");
            Database.WriteAttribute(StartRoom, "IN", "0");
            
            Database.WriteAttribute(DatabaseConstants.Money, "short", "dollar");
            Database.WriteAttribute(DatabaseConstants.Money, "nouns", "dollar:cash:bill");

            Database.CommitChanges();
        }
    }
}
