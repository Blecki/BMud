using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public interface IClientSource
    {
        void Listen(MudCore Core);
    }
}
