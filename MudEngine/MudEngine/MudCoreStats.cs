using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class MudCore
    {
        public static void GrantStat(MudObject Object, String Stat, Int32 Amount)
        {
            Int32 ExistingAmount = 0;
            Int32 MaxAmount = 0;

            try { ExistingAmount = Convert.ToInt32(Object.GetAttribute(Stat, "0")); }
            catch (Exception) { ExistingAmount = 0; }

            try { MaxAmount = Convert.ToInt32(Object.GetAttribute(Stat + "MAX", "0")); }
            catch (Exception) { MaxAmount = 0; }

            Int32 NewAmount = Math.Min(MaxAmount, ExistingAmount + Amount);

            Object.SetAttribute(Stat, NewAmount.ToString());
        }

        public static bool CostStat(MudObject Object, String Stat, Int32 Amount)
        {
            Int32 ExistingAmount = 0;
            try { ExistingAmount = Convert.ToInt32(Object.GetAttribute(Stat, "0")); }
            catch (Exception) { ExistingAmount = 0; }

            if (ExistingAmount - Amount < 0) return false;

            Object.SetAttribute(Stat, (ExistingAmount - Amount).ToString());
            return true;
        }
    }
}
