using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public interface IProtectedAttribute
    {
        bool Check(MudObject Actor, MudObject Object, String NewValue, IMessageService Message);
        bool CanDelete(MudObject Actor, MudObject Object, IMessageService Message);
    }
}
