using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Matchers
{
    public class Optional : ICommandTokenMatcher
    {
        ICommandTokenMatcher Matcher;

        public Optional(ICommandTokenMatcher Matcher) { this.Matcher = Matcher; }

        public List<PossibleMatch> Match(
            PossibleMatch _matchState,
            IDatabaseService _database,
            string _rawCommand)
        {
            var R = new List<PossibleMatch>();
            R.AddRange(Matcher.Match(_matchState, _database, _rawCommand));
            R.Add(_matchState);
            return R;
        }
    }
}
