using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public interface ScriptObject
    {
        Object GetProperty(String name);
        void SetProperty(String name, Object value);
    }

}
