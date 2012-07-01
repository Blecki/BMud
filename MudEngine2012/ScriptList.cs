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
                    return (o == null ? "null" : (o is ScriptObject ? o.GetType().Name : o.ToString()));
                })) + " }";
        }
    }
}
