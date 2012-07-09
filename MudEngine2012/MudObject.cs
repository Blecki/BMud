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
        public MudObject @base = null;

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

        public void CopyFrom(MudObject obj)
        {
            this.path = obj.path;
            this.@base = obj.@base;
            this.attributes.Clear();
            foreach (var attr in obj.attributes)
                this.attributes.Add(attr.Key, attr.Value);
        }

        public void ClearProperties() { attributes.Clear(); }

        private Dictionary<String, Object> attributes = new Dictionary<String, Object>();

        object GetInheritedProperty(string name, List<MudObject> inheritanceStack)
        {
            if (inheritanceStack.Contains(this)) return null;
            if (attributes.ContainsKey(name)) return attributes[name];
            inheritanceStack.Add(this);
            if (@base == null) return database.LoadObject("object").GetInheritedProperty(name, inheritanceStack);
            else return @base.GetInheritedProperty(name, inheritanceStack);
        }

        override public object GetProperty(string name)
        {
            if (name == "path") return path;
            if (name == "base") 
                return @base;
            if (attributes.ContainsKey(name)) return attributes[name];
            return GetInheritedProperty(name, new List<MudObject>());
        }

        override public void SetProperty(string name, Object value)
        {
            if (name == "path") throw new ScriptError("Path is a read-only property.");
            if (name == "base") @base = value as MudObject;
            else attributes.Upsert(name, value);
        }

        override public ScriptList ListProperties()
        {
            var r = new ScriptList(attributes.Select((p) => { return p.Key; }));
            r.Add("path");
            r.Add("base");
            return r;
        }

        override public void DeleteProperty(String name)
        {
            if (name == "path") throw new ScriptError("Path is a read-only property.");
            if (name == "base") throw new ScriptError("Can't delete base from mud object.");
            if (attributes.ContainsKey(name)) attributes.Remove(name);
        }
    }
}
