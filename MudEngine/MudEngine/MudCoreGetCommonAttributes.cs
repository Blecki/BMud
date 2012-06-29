using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public partial class MudCore
    {
        public static Int32 GetObjectRank(MudObject Object)
        {
            try
            {
                return Convert.ToInt32(Object.GetAttribute("RANK", "0"));
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static Int64 GetObjectOwner(MudObject Object)
        {
            try
            {
                return Convert.ToInt64(Object.GetAttribute("OWNER", "-1"));
            }
            catch (Exception)
            {
                return DatabaseConstants.Invalid;
            }
        }
    }
}
