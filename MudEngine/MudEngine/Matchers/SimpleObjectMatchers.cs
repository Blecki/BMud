using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Matchers
{
    public class ObjectID : ICommandTokenMatcher
    {
        String ArgumentName;

        public ObjectID(String ArgumentName) { this.ArgumentName = ArgumentName; }

        public List<PossibleMatch> Match(
            PossibleMatch _matchState,
            IDatabaseService _database,
            string _rawCommand)
        {
            var R = new List<PossibleMatch>();
            if (_matchState._next == null) return R;

            try
            {
                Int64 ID = Convert.ToInt64(_matchState._next.Value.Value);
                var Actor = _matchState.GetArgument<MudObject>("ACTOR", null);
                var Object = MudObject.FromID(ID, _database);
                R.Add(new PossibleMatch(_matchState._arguments, _matchState._next.Next, ArgumentName, Object));
            }
            catch (Exception) { }
            return R;
        }
    }

    public class Here : ICommandTokenMatcher
    {
        String ArgumentName;
        public Here(String ArgumentName) { this.ArgumentName = ArgumentName; }

        public List<PossibleMatch> Match(PossibleMatch _matchState, IDatabaseService _database, string _rawCommand)
        {
            var R = new List<PossibleMatch>();
            if (_matchState._next == null) return R;
            if (_matchState._next.Value.Value.ToUpper() == "HERE")
                R.Add(new PossibleMatch(_matchState._arguments, _matchState._next.Next, ArgumentName,
                    _matchState.GetArgument<MudObject>("ACTOR", null).Location.Parent));
            return R;
        }
    }

    public class Me : ICommandTokenMatcher
    {
        String ArgumentName;
        public Me(String ArgumentName) { this.ArgumentName = ArgumentName; }

        public List<PossibleMatch> Match(PossibleMatch _matchState, IDatabaseService _database, string _rawCommand)
        {
            var R = new List<PossibleMatch>();
            if (_matchState._next == null) return R;
            if (_matchState._next.Value.Value.ToUpper() == "ME")
                R.Add(new PossibleMatch(_matchState._arguments, _matchState._next.Next, ArgumentName,
                    _matchState.GetArgument<MudObject>("ACTOR", null)));
            return R;
        }
    }

    public class AllObjects : OrMatcher
    {
        public AllObjects(String ArgumentName) : base(
            new ObjectM(new OSEverything("ACTOR"), ArgumentName), new ObjectID(ArgumentName), new Here(ArgumentName),
            new Me(ArgumentName))
        {   
        }
    }

}
