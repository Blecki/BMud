using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public class MatchedObject
    {
        public String Text;
        public List<MudObject> Objects;

        public MatchedObject(String Text, List<MudObject> Objects)
        {
            this.Text = Text;
            this.Objects = Objects;
        }
    }

    namespace Matchers
    {
        public class ObjectM : ICommandTokenMatcher
        {
            IObjectSource Source;
            String ArgumentName;

            public ObjectM(IObjectSource Source, String ArgumentName)
            {
                this.Source = Source;
                this.ArgumentName = ArgumentName;
            }

            public List<PossibleMatch> Match(
                PossibleMatch _matchState,
                IDatabaseService _database,
                string _rawCommand)
            {
                if (_matchState._next == null) return new List<PossibleMatch>();

                var StartToken = _matchState._next;
                Int32 Nth = -1;

                if (StartToken.Value.Value.Length > 2)
                {
                    String Suffix = StartToken.Value.Value.Substring(StartToken.Value.Value.Length - 2).ToUpper();
                    if (Suffix == "ST" || Suffix == "ND" || Suffix == "RD" || Suffix == "TH")
                    {
                        String Number = StartToken.Value.Value.Substring(0, StartToken.Value.Value.Length - 2);
                        try
                        {
                            Nth = Convert.ToInt32(Number);
                            StartToken = StartToken.Next;
                        }
                        catch (Exception) { }
                    }
                }
                    

                //Find the set of objects with the longest match
                int TokensMatched = 0;
                List<MudObject> MatchedObjects = new List<MudObject>();

                var SearchSet = Source.GetObjects(_matchState, _database);

                #region Find Longest Matches
                foreach (var Object in SearchSet)
                {
                    var Nouns = new List<String>(Object.GetAttribute("NOUNS", "").Split(new char[] { ':' },
                        StringSplitOptions.RemoveEmptyEntries));
                    var Adjectives = new List<String>(Object.GetAttribute("ADJECTIVES", "").Split(new char[] { ':' },
                         StringSplitOptions.RemoveEmptyEntries));

                    var NextToken = StartToken;
                    int _tokensMatched = 0;
                    while (NextToken != null)
                    {
                        if (Nouns.Find((A) => { return A.ToUpper() == NextToken.Value.Value.ToUpper(); }) != null)
                        {
                            _tokensMatched += 1;
                            if (_tokensMatched == TokensMatched)
                                MatchedObjects.Add(Object);
                            else if (_tokensMatched > TokensMatched)
                            {
                                TokensMatched = _tokensMatched;
                                MatchedObjects.Clear();
                                MatchedObjects.Add(Object);
                            }
                            break;
                        }
                        else if (Adjectives.Find((A) => { return A.ToUpper() == NextToken.Value.Value.ToUpper(); }) != null)
                        {
                            NextToken = NextToken.Next;
                            _tokensMatched += 1;
                        }
                        else break;
                    }
                }
                #endregion

                String Text = "";
                var AfterToken = StartToken;
                for (int i = 0; i < TokensMatched; ++i)
                {
                    if (!String.IsNullOrEmpty(Text)) Text += " ";
                    Text += AfterToken.Value.Value;
                    AfterToken = AfterToken.Next;
                }

                if (MatchedObjects.Count == 0) return new List<PossibleMatch>();
                var R = new List<PossibleMatch>();

                if (Nth != -1)
                {
                    if (Nth <= 0 || Nth > MatchedObjects.Count)
                        return R;
                    R.Add(new PossibleMatch(_matchState._arguments, AfterToken, ArgumentName, MatchedObjects[Nth - 1]));
                }
                else
                    R.Add(new PossibleMatch(_matchState._arguments, AfterToken, ArgumentName, MatchedObjects[0]));

                //R.Add(new PossibleMatch(_matchState._arguments, AfterToken, ArgumentName, new MatchedObject(Text, MatchedObjects)));
                return R;
            }
        }
    }
}