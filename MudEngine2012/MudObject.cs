using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class MudObject : ScriptObject
    {
        public Database database { get; private set; }
        public String path { get; private set; }

        public MudObject(Database database, String path)
        {
            this.database = database;
            this.path = path;
        }

        public MudObject(Database database)
        {
            this.database = database;
            this.path = "";
        }

        private Dictionary<String, Object> attributes = new Dictionary<String, Object>();

        object ScriptObject.GetProperty(string name)
        {
            if (name == "path") return path;
            if (attributes.ContainsKey(name)) return attributes[name];
            return null;
        }

        void ScriptObject.SetProperty(string name, Object value)
        {
            if (name == "path") throw new ScriptError("Path is a read-only property.");
            attributes.Upsert(name, value);
        }

        ScriptList ScriptObject.ListProperties()
        {
            var r = new ScriptList(attributes.Select((p) => { return p.Key; }));
            r.Add("path");
            return r;
        }

        void ScriptObject.DeleteProperty(String name)
        {
            if (name == "path") throw new ScriptError("Path is a read-only property.");
            if (attributes.ContainsKey(name)) attributes.Remove(name);
        }
    }
}
