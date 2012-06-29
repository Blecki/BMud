using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Matchers
{
    public class Push : ICommandTokenMatcher
    {
        Func<PossibleMatch, Object> _func;
        String ArgumentName;

        public Push(String ArgumentName, Func<PossibleMatch, Object> _func)
        {
            this.ArgumentName = ArgumentName;
            this._func = _func;
        }

        public List<PossibleMatch> Match(
            PossibleMatch _matchState,
            IDatabaseService _database,
            string _rawCommand)
        {
            var R = new List<PossibleMatch>();
            _matchState.Upsert(ArgumentName, _func(_matchState));
            R.Add(_matchState);
            return R;
        }
    }
}
