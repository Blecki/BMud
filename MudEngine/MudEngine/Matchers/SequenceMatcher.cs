using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Matchers
{
    public class Sequence : ICommandTokenMatcher
    {
        internal List<ICommandTokenMatcher> Matchers = new List<ICommandTokenMatcher>();

        public Sequence(ICommandTokenMatcher First, ICommandTokenMatcher Second, ICommandTokenMatcher Third = null,
            ICommandTokenMatcher Fourth = null)
        {
            Matchers.Add(First);
            Matchers.Add(Second);
            if (Third != null) Matchers.Add(Third);
            if (Fourth != null) Matchers.Add(Fourth);
        }

        public List<PossibleMatch> Match(
            PossibleMatch _matchState,
            IDatabaseService _database,
            String _rawCommand)
        {
            var Matches = new List<PossibleMatch>();
            Matches.Add(_matchState);
            foreach (var Matcher in Matchers)
            {
                var NextMatches = new List<PossibleMatch>();
                foreach (var Match in Matches)
                    NextMatches.AddRange(Matcher.Match(Match, _database, _rawCommand));
                Matches = NextMatches;
            }
            return Matches;
        }
    }

    public class OrMatcher : ICommandTokenMatcher
    {
        internal ICommandTokenMatcher First;
        internal ICommandTokenMatcher Second;
        internal ICommandTokenMatcher Third;
        internal ICommandTokenMatcher Fourth;

        public OrMatcher(ICommandTokenMatcher First, ICommandTokenMatcher Second, 
            ICommandTokenMatcher Third = null,
            ICommandTokenMatcher Fourth = null)
        {
            this.First = First;
            this.Second = Second;
            this.Third = Third;
            this.Fourth = Fourth;
        }

        public List<PossibleMatch> Match(
            PossibleMatch _matchState,
            IDatabaseService _database,
            String _rawCommand)
        {
            var Matches = First.Match(_matchState, _database, _rawCommand);
            Matches.AddRange(Second.Match(_matchState, _database, _rawCommand));
            if (Third != null) Matches.AddRange(Third.Match(_matchState, _database, _rawCommand));
            if (Fourth != null) Matches.AddRange(Fourth.Match(_matchState, _database, _rawCommand));
            return Matches;
        }
    }
}
