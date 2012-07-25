using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class DeserializeError : MISP.ScriptError
    {
        public DeserializeError() : base("Invalid serialized object", null) {}
    }

    public class ObjectDeserializer
    {
        internal class SerializerState
        {
            internal Dictionary<uint, MISP.GenericScriptObject> referencedObjects = 
                new Dictionary<uint,MISP.GenericScriptObject>();

            internal MISP.ScriptObject ReadObject(ReadOnlyDatagram datagram, SerializedTypeCode typeCode, Database database)
            {
                if (typeCode == SerializedTypeCode.NamedObject)
                {
                    String objectName = "";
                    if (!datagram.ReadString(out objectName)) throw new DeserializeError();
                    return database.LoadObject(objectName);
                }
                else if (typeCode == SerializedTypeCode.InternalObject)
                {
                    uint id = 0;
                    if (!datagram.ReadUInt(out id, 16)) throw new DeserializeError();
                    if (referencedObjects.ContainsKey(id)) return referencedObjects[id];
                    else
                    {
                        var obj = new MISP.GenericScriptObject();
                        referencedObjects.Add(id, obj);
                        return obj;
                    }
                }
                else
                    throw new DeserializeError();
            }
        }

        internal static Object DeserializeObject(ReadOnlyDatagram datagram, SerializerState state, Database database)
        {
            uint rawTypeCode = 0;
            if (!datagram.ReadUInt(out rawTypeCode, 8)) throw new DeserializeError();
            switch ((SerializedTypeCode)rawTypeCode)
            {
                case SerializedTypeCode.Null:
                    return null;
                case SerializedTypeCode.Integer:
                    {
                        uint value = 0;
                        if (!datagram.ReadUInt(out value, 32)) throw new DeserializeError();
                        return (int)value;
                    }
                case SerializedTypeCode.String:
                    {
                        String value = "";
                        if (!datagram.ReadString(out value)) throw new DeserializeError();
                        return value;
                    }
                case SerializedTypeCode.List:
                    {
                        uint length = 0;
                        if (!datagram.ReadUInt(out length, 16)) throw new DeserializeError();
                        var result = new MISP.ScriptList();
                        for (int i = 0; i < length; ++i)
                            result.Add(DeserializeObject(datagram, state, database));
                        return result;
                    }
                case SerializedTypeCode.InternalObject:
                    return state.ReadObject(datagram, SerializedTypeCode.InternalObject, database);
                case SerializedTypeCode.NamedObject:
                    return state.ReadObject(datagram, SerializedTypeCode.NamedObject, database);
                default:
                    throw new DeserializeError();
            }
        }

        internal static void impleDeserialize(ReadOnlyDatagram datagram, SerializerState state, Database database)
        {
            uint index = 0;
            if (!datagram.ReadUInt(out index, 16)) throw new DeserializeError();
            MISP.GenericScriptObject into = null;
            if (state.referencedObjects.ContainsKey(index)) into = state.referencedObjects[index];
            else
            {
                into = new MISP.GenericScriptObject();
                state.referencedObjects.Add(index, into);
            }

            uint propertyCount = 0;
            if (!datagram.ReadUInt(out propertyCount, 16)) throw new DeserializeError();
            for (int i = 0; i < propertyCount; ++i)
            {
                String propertyName = "";
                Object value = null;
                if (!datagram.ReadString(out propertyName)) throw new DeserializeError();
                value = DeserializeObject(datagram, state, database);
                into.SetProperty(propertyName, value);
            }
        }

        public static void Deserialize(MISP.GenericScriptObject into, ReadOnlyDatagram from, Database database)
        {
            var state = new SerializerState();
            state.referencedObjects.Add(0, into);
            while (from.More)
                impleDeserialize(from, state, database);
        }
    }
}
