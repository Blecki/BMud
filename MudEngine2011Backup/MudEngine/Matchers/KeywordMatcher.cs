using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Matchers
{
    internal class KeyWord : ICommandTokenMatcher
    {
        public String _word;
        public bool _optional = false;

        internal KeyWord(String _word, bool _optional)
        {
            this._word = _word.ToUpper();
            this._optional = _optional;
        }

        public List<PossibleMatch> Match(
            PossibleMatch _matchState,
            IDatabaseService _database,
            string _rawCommand)
        {
            var R = new List<PossibleMatch>();
            if (_matchState._next != null && _matchState._next.Value.Value.ToUpper() == _word)
                R.Add(new PossibleMatch(_matchState._arguments, _matchState._next.Next));
            if (_optional)
                R.Add(new PossibleMatch(_matchState._arguments, _matchState._next));
            return R;
        }
    }
}
