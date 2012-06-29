using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Matchers
{
    public class Flipper : ICommandTokenMatcher
    {
        ICommandTokenMatcher First;
        ICommandTokenMatcher Middle;
        ICommandTokenMatcher Last;

        public Flipper(ICommandTokenMatcher First, ICommandTokenMatcher Middle,
            ICommandTokenMatcher Last)
        {
            this.First = First;
            this.Middle = Middle;
            this.Last = Last;
        }

        public List<PossibleMatch> Match(
            PossibleMatch _matchState,
            IDatabaseService _database,
            string _rawCommand)
        {
            var FinalResults = new List<PossibleMatch>();
            var NextNode = _matchState._next;

            while (NextNode != null)
            {
                var MiddleResults = Middle.Match(new PossibleMatch(_matchState._arguments, NextNode), _database, _rawCommand);

                foreach (var MResult in MiddleResults)
                {
                    var LastResults = Last.Match(MResult, _database, _rawCommand);
                    foreach (var LResult in LastResults)
                    {
                        var TempTokenList = new LinkedList<CommandParser.Token>();
                        for (var TNode = _matchState._next; TNode != NextNode; TNode = TNode.Next)
                            TempTokenList.AddLast(TNode.Value);

                        var FirstResults = First.Match(new PossibleMatch(LResult._arguments, TempTokenList.First), 
                            _database, _rawCommand);

                        foreach (var FResult in FirstResults)
                            if (FResult._next == null)
                                FinalResults.Add(new PossibleMatch(FResult._arguments, LResult._next));
                    }
                }

                NextNode = NextNode.Next;
            }

            return FinalResults;
        }
    }
}
