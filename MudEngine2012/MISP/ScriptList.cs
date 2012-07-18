using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012.MISP
{
    public class ScriptList : List<Object>, ScriptAsString
    {
        public ScriptList(IEnumerable<Object> collection) : base(collection) { }
        public ScriptList() { }

        public ScriptList(params Object[] items)
        {
            foreach (var item in items) Add(item);
        }

        public String AsString(int depth)
        {
            if (depth < 0) return "L" + Count;
            return "L" + Count + "{" + String.Join(", ", this.Select((o) =>
                {
                    return ScriptObject.AsString(o, depth - 1);
                })) + " }";
        }
    }
}
