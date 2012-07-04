using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class ScriptList : List<Object>, ScriptAsString
    {
        public ScriptList(IEnumerable<Object> collection) : base(collection) { }
        public ScriptList() { }

        public String AsString(int depth)
        {
            if (depth > 1) return "L" + Count;
            return "L" + Count + "{" + String.Join(", ", this.Select((o) =>
                {
                    return ScriptObject.AsString(o, depth + 1);
                })) + " }";
        }
    }
}
