using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public interface ScriptObject
    {
        Object GetProperty(String name);
        void SetProperty(String name, Object value);
        void DeleteProperty(String name);
        ScriptList ListProperties();
    }

    public class ReflectionScriptObject : ScriptObject
    {
        object ScriptObject.GetProperty(string name)
        {
            var field = this.GetType().GetField(name);
            if (field != null) return field.GetValue(this);
            throw new ScriptError("Objects of type " + this.GetType().ToString() + " do not have a member named " + name + ".");
        }

        void ScriptObject.DeleteProperty(String name)
        {
            throw new ScriptError("Objects of type " + this.GetType().ToString() + " are read-only.");
        }

        void ScriptObject.SetProperty(string name, object value)
        {
            throw new ScriptError("Objects of type " + this.GetType().ToString() + " are read-only.");
        }

        ScriptList ScriptObject.ListProperties()
        {
            return new ScriptList(this.GetType().GetFields().Select((info) => { return info.Name; }));
        }

        public override string ToString()
        {
            return "RSO{" + String.Join(", ", (this as ScriptObject).ListProperties().Select((o) =>
                {
                    var prop = (this as ScriptObject).GetProperty(o.ToString());
                    return o.ToString() + ": " + (prop == null ? "null" : (prop is ScriptObject ? prop.GetType().Name : prop.ToString()));
                })) + " }";
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
                (this as ScriptObject).SetProperty(args[i].ToString(), args[i + 1]);
        }

        object ScriptObject.GetProperty(string name)
        {
            if (properties.ContainsKey(name)) return properties[name];
            else throw new ScriptError("No property with name " + name + " on object.");
        }

        void ScriptObject.SetProperty(string Name, Object Value)
        {
            if (properties.ContainsKey(Name)) properties[Name] = Value;
            else properties.Add(Name, Value);
        }

        void ScriptObject.DeleteProperty(String Name)
        {
            if (properties.ContainsKey(Name)) properties.Remove(Name);
        }

        ScriptList ScriptObject.ListProperties()
        {
            return new ScriptList(properties.Select((p) => { return p.Key; }));
        }

        public override string ToString()
        {
            return "GSO{" + String.Join(", ", properties.Select((p) =>
            {
                return p.Key + ": " +
                    (p.Value == null ? "null" : (p.Value is ScriptObject ? p.Value.GetType().Name : p.Value.ToString()));
            })) + " }";
        }
    }


}
