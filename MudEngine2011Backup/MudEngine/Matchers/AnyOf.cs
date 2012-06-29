using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Matchers
{
    public class AnyOf : ICommandTokenMatcher
    {
        List<String> Words;
        String ArgumentName;
        
        public AnyOf(List<String> Words, String ArgumentName)
        {
            this.Words = Words;
            this.ArgumentName = ArgumentName;
        }

        public List<PossibleMatch> Match(
            PossibleMatch _matchState, 
            IDatabaseService _database, 
            string _rawCommand)
        {
            var R = new List<PossibleMatch>();
            if (_matchState._next != null)
            {
                String Word = _matchState._next.Value.Value.ToUpper();
                if (Words.Find((A) => { return A == Word; }) != null)
                    R.Add(new PossibleMatch(_matchState._arguments, _matchState._next.Next, ArgumentName, Word));
            }
            return R;
        }
    }

    public class InOnUnder : AnyOf
    {
        public InOnUnder(String ArgumentName) : base(new List<string> { "IN", "ON", "UNDER" }, ArgumentName) { }
    }
}
