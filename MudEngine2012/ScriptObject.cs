using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public interface ScriptAsString
    {
        String AsString(int depth);
    }

    public class ScriptObject : ScriptAsString
    {
        public virtual Object GetProperty(String name) { return null; }
        public virtual void SetProperty(String name, Object value) { }
        public virtual void DeleteProperty(String name) { }
        public virtual ScriptList ListProperties() { return new ScriptList(); }

        public String AsString(int depth)
        {
            if (depth < 0) return this.GetType().Name;
            else return "SO{" + String.Join(", ", (this as ScriptObject).ListProperties().Select((o) =>
            {
                return o.ToString() + ": " + ScriptObject.AsString(this.GetProperty(o.ToString()), depth - 1);
            })) + " }";
        }

        public static String AsString(Object obj, int depth = 0)
        {
            if (obj == null) return "null";
            if (obj is ScriptAsString) return (obj as ScriptAsString).AsString(depth);
            return obj.ToString();
        }
    }

    public class ReflectionScriptObject : ScriptObject
    {
        override public object GetProperty(string name)
        {
            var field = this.GetType().GetField(name);
            if (field != null) return field.GetValue(this);
            return null;
        }

        override public void DeleteProperty(String name)
        {
            throw new ScriptError("Objects of type " + this.GetType().Name + " are read-only.");
        }

        override public void SetProperty(string name, object value)
        {
            throw new ScriptError("Objects of type " + this.GetType().Name + " are read-only.");
        }

        override public ScriptList ListProperties()
        {
            return new ScriptList(this.GetType().GetFields().Select((info) => { return info.Name; }));
        }
    }

    public class GenericScriptObject : ScriptObject
    {
        public Dictionary<String, Object> properties = new Dictionary<string, object>();

        public GenericScriptObject() { }

        public GenericScriptObject(ScriptObject cloneFrom)
        {
            foreach (var str in cloneFrom.ListProperties())
                (this as ScriptObject).SetProperty(str as String, cloneFrom.GetProperty(str as String));
        }

        public GenericScriptObject(params Object[] args)
        {
            if (args.Length % 2 != 0) throw new InvalidProgramException("Generic Script Object must be initialized with pairs");
            for (int i = 0; i < args.Length; i += 2)
                SetProperty(ScriptObject.AsString(args[i]), args[i + 1]);
        }

        override public object GetProperty(string name)
        {
            if (properties.ContainsKey(name)) return properties[name];
            else return null;
        }

        override public void SetProperty(string Name, Object Value)
        {
            if (properties.ContainsKey(Name)) properties[Name] = Value;
            else properties.Add(Name, Value);
        }

        override public void DeleteProperty(String Name)
        {
            if (properties.ContainsKey(Name)) properties.Remove(Name);
        }

        override public ScriptList ListProperties()
        {
            return new ScriptList(properties.Select((p) => { return p.Key; }));
        }

    }


}
