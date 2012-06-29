using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    internal class OperationLimit
    {
        internal int _limit;
        public bool Dec()
        {
            _limit -= 1;
            return _limit < 0;
        }

        public void DecThrow()
        {
            if (Dec()) throw new OperationLimitExceededException();
        }
    }

    public class OperationLimitExceededException : Exception
    {
    }
}
