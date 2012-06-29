using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Matchers
{
    public class IntegerM : ICommandTokenMatcher
    {
        String ArgumentName;

        public IntegerM(String ArgumentName) { this.ArgumentName = ArgumentName; }

        public List<PossibleMatch> Match(
            PossibleMatch _matchState, 
            IDatabaseService _database, 
            string _rawCommand)
        {
            var R = new List<PossibleMatch>();
            if (_matchState._next == null) return R;

            try
            {
                Int32 Number = Convert.ToInt32(_matchState._next.Value.Value);
                R.Add(new PossibleMatch(_matchState._arguments, _matchState._next.Next, ArgumentName, Number));
            }
            catch (Exception) { }
            return R;
        }
    }
}
