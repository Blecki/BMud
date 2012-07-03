using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class ScriptList : List<Object>
    {
        public ScriptList(IEnumerable<Object> collection) : base(collection) { }
        public ScriptList() { }

        public override string ToString()
        {
            return "L" + Count.ToString() + "{" + String.Join(", ", this.Select((o) =>
                {
                    if (o == null) return "null";
                    if (o is ScriptObject) return o.GetType().Name;
                    if (o is ScriptList) return o.GetType().Name;
                    return o.ToString();
                })) + " }";
        }
    }
}
