using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class MudCore
    {
        public static void RecycleID(Int64 ID, IDatabaseService _database)
        {
            var God = MudObject.FromID(DatabaseConstants.God, _database);
            var ObList = God.GetContents("RECYCLED");
            var Entry = ObList.Find((A) => { return A.ID == ID; });
            if (Entry != null) return;
            God.AddChild(MudObject.FromID(ID, _database), "RECYCLED");
        }

        public static Int64 AllocateID(IDatabaseService _database)
        {
            var God = MudObject.FromID(DatabaseConstants.God, _database);
            var ObList = God.GetContents("RECYCLED");
            if (ObList.Count == 0) return _database.CreateObject();
            var Result = ObList[0].ID;
            God.RemoveChild("RECYCLED", 0, 1);
            return Result;
        }
    }
}
