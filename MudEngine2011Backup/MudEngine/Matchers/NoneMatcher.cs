using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Matchers
{
    public class None : ICommandTokenMatcher
    {
        public List<PossibleMatch> Match(
            PossibleMatch _matchState, 
            IDatabaseService _database, 
            string _rawCommand)
        {
            var R = new List<PossibleMatch>();
            if (_matchState._next == null)
                R.Add(new PossibleMatch(_matchState._arguments, null));
            return R;
        }
    }
}
