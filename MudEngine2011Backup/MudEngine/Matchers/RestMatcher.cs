using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Matchers
{
    public class Rest : ICommandTokenMatcher
    {
        String ArgumentName;

        public Rest(String ArgumentName) 
        { 
            this.ArgumentName = ArgumentName;
        }

        public List<PossibleMatch> Match(
            PossibleMatch _matchState,
            IDatabaseService _database, 
            string _rawCommand)
        {
            var R = new List<PossibleMatch>();
            if (_matchState._next != null)
                R.Add(new PossibleMatch(_matchState._arguments, null, ArgumentName,
                    _rawCommand.Substring(_matchState._next.Value.Place)));
            return R;
        }
    }
}
