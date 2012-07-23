using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    internal enum SerializedTypeCode
    {
        Null,
        String,
        Integer,
        List,
        NamedObject,
        InternalObject,
    }

    public class ObjectSerializer
    {
        internal class SerializerState
        {
            internal Dictionary<MISP.ScriptObject, uint> referencedObjectsIDs = new Dictionary<MISP.ScriptObject, uint>();
            internal List<MISP.ScriptObject> referencedObjects = new List<MISP.ScriptObject>();

            internal void WriteObject(MISP.ScriptObject obj, WriteOnlyDatagram datagram)
            {
                var path = obj.GetProperty("@path"); //If it has a path attribute, it's a named object and should not be saved.
                if (path != null && path is String)
                {
                    datagram.WriteUInt((uint)SerializedTypeCode.NamedObject, 8);
                    datagram.WriteString(path as String);
                }
                else
                {
                    datagram.WriteUInt((uint)SerializedTypeCode.InternalObject, 8);
                    if (referencedObjectsIDs.ContainsKey(obj))
                        datagram.WriteUInt(referencedObjectsIDs[obj], 16);
                    else
                    {
                        referencedObjectsIDs.Add(obj, (uint)referencedObjects.Count);
                        referencedObjects.Add(obj);
                        datagram.WriteUInt((uint)(referencedObjects.Count - 1), 16);
                    }
                }
            }
        }

        internal static bool IsSerializableType(Object obj)
        {
            if (obj is MISP.GenericScriptObject) return true;
            if (obj is String) return true;
            if (obj is int) return true;
            if (obj == null) return true;
            if (obj is MISP.ScriptList) return true;
            return false;
        }

        internal static void SerializeObject(Object obj, WriteOnlyDatagram datagram, SerializerState state)
        {
            if (obj == null)
            {
                datagram.WriteUInt((uint)SerializedTypeCode.Null, 8);
            }
            else if (obj is String)
            {
                datagram.WriteUInt((uint)SerializedTypeCode.String, 8);
                datagram.WriteString(obj as String);
            }
            else if (obj is MISP.ScriptList)
            {
                datagram.WriteUInt((uint)SerializedTypeCode.List, 8);
                var filteredList = new MISP.ScriptList(
                    (obj as MISP.ScriptList).Where((o) => IsSerializableType(o)));
                datagram.WriteUInt((uint)filteredList.Count, 16);
                foreach (var item in filteredList)
                    SerializeObject(item, datagram, state);
            }
            else if (obj is int)
            {
                datagram.WriteUInt((uint)SerializedTypeCode.Integer, 8);
                datagram.WriteUInt((uint)(obj as int?).Value, 32);
            }
            else if (obj is MISP.GenericScriptObject)
            {
                state.WriteObject(obj as MISP.GenericScriptObject, datagram);
            }
        }

        internal static void impleSerialize(MISP.ScriptObject obj, int index, WriteOnlyDatagram datagram, SerializerState state)
        {
            datagram.WriteUInt((uint)index, 16);

            var propList = obj.ListProperties();
            var filteredList = new List<Object>(
                propList.Where((o) => { return IsSerializableType(obj.GetLocalProperty(o as String)); }));

            datagram.WriteUInt((uint)filteredList.Count, 16);
            foreach (var item in filteredList)
            {
                datagram.WriteString(item as String);
                SerializeObject(obj.GetLocalProperty(item as String), datagram, state);
            }
        }

        public static WriteOnlyDatagram Serialize(MISP.ScriptObject obj)
        {
            var r = new WriteOnlyDatagram();
            var state = new SerializerState();
            state.referencedObjects.Add(obj);
            state.referencedObjectsIDs.Add(obj, 0);

            for (int i = 0; i < state.referencedObjects.Count; ++i)
                impleSerialize(state.referencedObjects[i], i, r, state);

            return r;
        }
    }
}
