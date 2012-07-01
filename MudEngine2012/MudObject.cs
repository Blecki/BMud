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

        public Object GetAttribute(String name) { return attributes[name]; }
        public bool HasAttribute(String name) { return attributes.ContainsKey(name); }
        public void SetAttribute(String name, Object value) { attributes.Upsert(name, value); }

        object ScriptObject.GetProperty(string name)
        {
            if (name == "path") return path;
            if (HasAttribute(name)) return GetAttribute(name);
            return null;
        }

        void ScriptObject.SetProperty(string name, Object value)
        {
            if (name == "path") throw new ScriptError("Path is a read-only property.");
            SetAttribute(name, value);
        }

        ScriptList ScriptObject.ListProperties()
        {
            var r = new ScriptList(attributes.Select((p) => { return p.Key; }));
            r.Add("path");
            return r;
        }
    }
}
