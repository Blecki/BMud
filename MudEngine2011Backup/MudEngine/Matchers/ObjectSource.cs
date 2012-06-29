using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine.Matchers
{
    public interface IObjectSource
    {
        List<MudObject> GetObjects(PossibleMatch _matchState, IDatabaseService _database);
    }

    public class OSAnd : IObjectSource
    {
        IObjectSource First, Second;

        public OSAnd(IObjectSource First, IObjectSource Second) { this.First = First; this.Second = Second; }

        public List<MudObject> GetObjects(PossibleMatch _matchState, IDatabaseService _database)
        {
            var R = First.GetObjects(_matchState, _database);
            R.AddRange(Second.GetObjects(_matchState, _database));
            return R;
        }
    }

    public class OSContents : IObjectSource
    {
        String _relative;
        List<String> _attributes;

        public OSContents(String Relative, String Attribute)
        {
            _relative = Relative;
            _attributes = new List<String>(Attribute.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public List<MudObject> GetObjects(PossibleMatch _matchState, IDatabaseService _database)
        {
            var R = new List<MudObject>();
            var Relative = _matchState.GetArgumentAsObjectList(_relative);
            foreach (var Rel in Relative)
                foreach (var Item in _attributes)
                    R.AddRange(Rel.GetContents(Item));
            return R;
        }
    }

    public class OSContentsSpec : IObjectSource
    {
        String _relative;
        String _attribute;

        public OSContentsSpec(String Rel, String Attr)
        {
            _relative = Rel;
            _attribute = Attr;
        }

        public List<MudObject> GetObjects(PossibleMatch _matchState, IDatabaseService _database)
        {
            var Relative = _matchState.GetArgumentAsObjectList(_relative);
            var Attribute = _matchState.GetArgument<String>(_attribute, null);

            if (Attribute == null) return new List<MudObject>();

            var R = new List<MudObject>();
            foreach (var Rel in Relative)
                R.AddRange(Rel.GetContents(Attribute));
            return R;
        }
    }

    public class OSLocation : IObjectSource
    {
        String _relative;
        List<String> _attributes;

        public OSLocation(String Relative, String Attribute)
        {
            _relative = Relative;
            _attributes = new List<String>(Attribute.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public List<MudObject> GetObjects(PossibleMatch _matchState, IDatabaseService _database)
        {
            var R = new List<MudObject>();
            var Relative = _matchState.GetArgumentAsObjectList(_relative);
            foreach (var Rel in Relative)
                if (Rel.Location != null && Rel.Location.Parent != null)
                    foreach (var Item in _attributes)
                        R.AddRange(Rel.Location.Parent.GetContents(Item));
            return R;
        }
    }

    public class OSSHeldWornLoc : OSAnd
    {
        public OSSHeldWornLoc(String Relative)
            : base(new OSLocation(Relative, "IN"), new OSContents(Relative, "HELD:WORN"))
        { }
    }

    public class OSOnLocationContents : IObjectSource
    {
        String _relative;

        public OSOnLocationContents(String Relative) { _relative = Relative; }

        public List<MudObject> GetObjects(PossibleMatch _matchState, IDatabaseService _database)
        {
            var R = new List<MudObject>();
            var Relative = _matchState.GetArgumentAsObjectList(_relative);
            foreach (var Rel in Relative)
                if (Rel.Location != null && Rel.Location.Parent != null)
                    foreach (var Object in Rel.Location.Parent.GetContents("IN"))
                        R.AddRange(Object.GetContents("ON"));
            return R;
        }
    }

    public class OSEverything : OSAnd
    {
        public OSEverything(String Relative)
            : base(new OSAnd(new OSLocation(Relative, "IN"), new OSOnLocationContents(Relative)),
            new OSContents(Relative, "HELD:WORN")) { }
    }
}