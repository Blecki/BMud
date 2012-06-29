using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public static class HelperExtensions
    {
        public static void Upsert<A,B>(this Dictionary<A,B> Dict, A _a, B _b)
        {
            if (Dict.ContainsKey(_a)) Dict[_a] = _b;
            else Dict.Add(_a, _b);
        }

        
    }

    public class MudObject
    {
        public Int64 ID { get; private set; }
        public bool Instance { get; private set; }
        public bool Stacked { get { return Count > 1; } }
        public bool Valid { get { return ID != DatabaseConstants.Invalid; } }

        public int Count
        {
            get
            {
                return Convert.ToInt32(GetLocalAttribute("COUNT", "1"));
            }

            set
            {
                SetLocalAttribute("COUNT", value.ToString());
            }
        }

        public class _Location
        {
            public MudObject Parent;
            public String List;
            public int Index;

            public _Location(MudObject Parent, String List, int Index)
            {
                this.Parent = Parent;
                this.List = List;
                this.Index = Index;
            }
        }

        public _Location Location;

        private IDatabaseService _database;
        private Dictionary<String, String> Attributes = new Dictionary<String, String>();
        private Dictionary<String, List<MudObject>> Contents = new Dictionary<String, List<MudObject>>();

        public List<MudObject> GetContents(String List)
        {
            if (Contents.ContainsKey(List.ToUpper())) return Contents[List.ToUpper()];
            var ObjectList = new List<MudObject>();
            Contents.Add(List.ToUpper(), ObjectList);
            var ParsedList = ParseList(GetAttribute(List, ""));
            foreach (var Item in ParsedList)
                ObjectList.Add(MudObject.FromString(Item, new _Location(this, List.ToUpper(), ObjectList.Count), _database));
            return ObjectList;
        }

        public void ReparseContents(String List)
        {
            if (Contents.ContainsKey(List.ToUpper())) Contents.Remove(List.ToUpper());
            GetContents(List);
        }

        public bool IsContents(String List) { return Contents.ContainsKey(List.ToUpper()); }

        public void UpdateContentsAttribute(String List)
        {
            if (!Contents.ContainsKey(List.ToUpper()))
                throw new InvalidProgramException("It couldn't have changed if it's never been queried!");
            String Value = "";
            foreach (var Object in Contents[List.ToUpper()])
                Value += Object.ToString() + ":";

            SetAttribute(List, Value);
        }

        public override string ToString()
        {
            String R = "";
            if (Instance) R += "@";
            R += ID.ToString();
            if (Attributes.Count > 0)
            {
                R += "[";
                foreach (var Item in Attributes)
                {
                    R += Item.Key;
                    if (!String.IsNullOrEmpty(Item.Value)) R += "=[" + Item.Value + "]";
                    R += ":";
                }
                R += "]";
            }
            return R;
        }

        public String ParameterString()
        {
            String R = "";
            if (Attributes.Count > 0)
            {
                R += "[";
                foreach (var Item in Attributes)
                {
                    if (Item.Key != "COUNT")
                    {
                    R += Item.Key;
                    if (!String.IsNullOrEmpty(Item.Value)) R += "=[" + Item.Value + "]";
                    R += ":";
                    }
                }
                R += "]";
            }
            return R;
        }

        private static Dictionary<Int64, MudObject> LoadedObjects = new Dictionary<Int64, MudObject>();
        public static void DiscardObject(Int64 ID) { if (LoadedObjects.ContainsKey(ID)) LoadedObjects.Remove(ID); }

        public static MudObject FromID(Int64 ID, IDatabaseService _database)
        {
            if (LoadedObjects.ContainsKey(ID)) return LoadedObjects[ID];
            if (!_database.ValidID(ID)) throw new InvalidProgramException("Invalid object ID.");
            var R = new MudObject();
            R.ID = ID;
            R._database = _database;
            LoadedObjects.Add(ID, R);

            MudObject.DiscoverTree(R);
            return R;
        }
        
        public static MudObject FromString(String _str, _Location Location, IDatabaseService _database)
        {
            MudObject R;

            String FirstPart;
            String Parameters = "";
            bool HasParameters = false;
            int BracketIndex = _str.IndexOf('[');
            if (BracketIndex != -1)
            {
                FirstPart = _str.Substring(0, BracketIndex);
                Parameters = _str.Substring(BracketIndex + 1, _str.Length - BracketIndex - 2);
                HasParameters = true;
            }
            else FirstPart = _str;

            if (FirstPart[0] == '@')
            {
                R = new MudObject();
                R.Instance = true;
                R.ID = Convert.ToInt64(FirstPart.Substring(1));
                if (!_database.ValidID(R.ID)) throw new InvalidProgramException("Invalid Database ID");
                R._database = _database;
                R.Location = Location;
            }
            else
            {
                R = FromID(Convert.ToInt64(FirstPart), _database);
                R.Instance = false;
                R.Attributes.Clear();
                R.Location = Location;
            }

            if (HasParameters)
            {
                var ParameterList = ParseList(Parameters);
                foreach (var Item in ParameterList)
                {
                    int EqualIndex = Item.IndexOf('=');
                    String Key = "";
                    String Value = "";
                    if (EqualIndex != -1)
                    {
                        Key = Item.Substring(0, EqualIndex);
                        Value = Item.Substring(EqualIndex + 2, Item.Length - EqualIndex - 3);
                    }
                    else
                        Key = Item;
                    R.Attributes.Upsert(Key, Value);
                }
            }

            return R;
        }

        public bool HasAttribute(String Key)
        {
            if (Attributes.ContainsKey(Key.ToUpper())) return true;
            else return _database.HasAttribute(ID, Key);
        }

        public bool HasLocalAttribute(String Key)
        {
            if (Attributes.ContainsKey(Key.ToUpper())) return true;
            else return false;
        }

        public Dictionary<String, String> GetAllAttributes()
        {
            if (Instance) return null;
            return _database.GetAllAttributes(ID);
        }

        public Dictionary<String, String> GetAllLocalAttributes()
        {
            return Attributes;
        }

        public String GetAttribute(String Key, String DefaultValue)
        {
            Key = Key.ToUpper();
            if (Attributes.ContainsKey(Key)) return Attributes[Key];
            else return _database.QueryAttribute(ID, Key, DefaultValue);
        }

        public String GetLocalAttribute(String Key, String DefaultValue)
        {
            if (Attributes.ContainsKey(Key.ToUpper())) return Attributes[Key.ToUpper()];
            else return DefaultValue;
        }

        public void SetAttribute(String Key, String Value)
        {
            if (Instance) SetLocalAttribute(Key, Value);
            else _database.WriteAttribute(ID, Key, Value);
        }

        public void SetLocalAttribute(String Key, String Value)
        {
            Attributes.Upsert(Key.ToUpper(), Value);
            if (Location != null && Location.Parent != null)
                Location.Parent.UpdateContentsAttribute(Location.List);
        }

        public void DeleteAttribute(String Key)
        {
            if (Instance) DeleteLocalAttribute(Key);
            else _database.RemoveAttribute(ID, Key);
        }

        public void DeleteLocalAttribute(String Key)
        {
            if (Attributes.ContainsKey(Key.ToUpper())) Attributes.Remove(Key.ToUpper());
            if (Location != null && Location.Parent != null) 
                Location.Parent.UpdateContentsAttribute(Location.List);
        }

        private static List<String> ParseList(String Text)
        {
            var Result = new List<String>();

            int Place = 0;
            String Temp = "";
            int Depth = 0;
            while (Place < Text.Length)
            {
                if (Text[Place] == '[') ++Depth;
                if (Text[Place] == ']') --Depth;
                if (Text[Place] == ':' && Depth == 0)
                {
                    if (!String.IsNullOrEmpty(Temp)) Result.Add(Temp);
                    Temp = "";
                }
                else
                    Temp += Text[Place];
                ++Place;
            }
            if (!String.IsNullOrEmpty(Temp)) Result.Add(Temp);
            return Result;
        }

        //Find the object at the top of the tree, or the first non-instance encountered.
        public MudObject FindTopObject()
        {
            if (Instance && Location != null && Location.Parent != null) return Location.Parent.FindTopObject();
            return this;
        }

        public bool SearchUp(MudObject Object)
        {
            if (Location == null) return false;
            if (Location.Parent == null) return false;
            if (Location.Parent == Object) return true;
            return Location.Parent.SearchUp(Object);
        }

        public void UpdateLocation()
        {
            if (Instance) return;
            MudObject Object = null;
            if (Location != null && Location.Parent != null) Object = Location.Parent.FindTopObject();
            if (Object != null && Object != this)
                SetAttribute("LOCATION", Object.ID.ToString());
            else
                DeleteAttribute("LOCATION");
        }

        private void BuildTree(String List)
        {
            if (HasAttribute(List))
            {
                var Contents = GetContents(List);
                foreach (var Item in Contents)
                    Item.BuildTree();
            }
        }

        public void BuildTree()
        {
            BuildTree("IN");
            BuildTree("ON");
            BuildTree("UNDER");
            BuildTree("HELD");
            BuildTree("WORN");
        }

        public void RemoveChild(String List, int Index, int Count)
        {
            if (!Contents.ContainsKey(List.ToUpper())) throw new InvalidProgramException();
            var ObjectList = Contents[List.ToUpper()];
            var Object = ObjectList[Index];
            var ObjectCount = Object.Count;
            if (ObjectCount - Count <= 0)
            {
                ObjectList.RemoveAt(Index);
                for (int i = 0; i < ObjectList.Count; ++i)
                    ObjectList[i].Location.Index = i;
                UpdateContentsAttribute(List);
                Object.Location = new _Location(null, "", 0);
            }
            else
            {
                Object.SetLocalAttribute("COUNT", (ObjectCount - Count).ToString());
                UpdateContentsAttribute(List);
            }
        }

        public void Banish(int Count)
        {
            if (!Instance && Count != 1) throw new InvalidProgramException();
            if (Location == null || Location.Parent == null) return;
            Location.Parent.RemoveChild(Location.List, Location.Index, Count);
        }

        public enum AddResult
        {
            Added,
            Stacked,
            AlreadyThere,
        }

        public AddResult AddChild(MudObject Child, String List)
        {
            var ObjectList = GetContents(List);

            if (Child.Instance)
            {
                var StringRepresentation = Child.ParameterString();
                var ExistingItem = ObjectList.Find(
                    (A) => { return A.Instance && A.ID == Child.ID && A.ParameterString() == StringRepresentation; });
                if (ExistingItem != null)
                {
                    ExistingItem.SetAttribute("COUNT", (ExistingItem.Count + Child.Count).ToString());
                    UpdateContentsAttribute(List);
                    return AddResult.Stacked;
                }
            }
            else
            {
                var ExistingItem = ObjectList.Find(
                    (A) => { return !A.Instance && A.ID == Child.ID; });
                if (ExistingItem != null) return AddResult.AlreadyThere;
            }
            Child.Location = new _Location(this, List.ToUpper(), ObjectList.Count);
            ObjectList.Add(Child);
            UpdateContentsAttribute(List);
            return AddResult.Added;
        }

        public MudObject Instanciate(int Count)
        {
            var R = Duplicate();
            R.Instance = true;
            R.Count = Count;
            return R;
        }

        public MudObject Duplicate()
        {
            var R = new MudObject();
            R.ID = ID;
            R.Instance = Instance;
            R._database = _database;
            R.Attributes = new Dictionary<string, string>(Attributes);
            return R;
        }

        public static MudObject MoveObject(MudObject Object, MudObject To, String List, int Count)
        {
            if (Object.Location != null && Object.Location.Parent != null)
            {
                var OriginalCount = Object.Count;
                Object.Location.Parent.RemoveChild(Object.Location.List, Object.Location.Index, Count);
                if (Count < OriginalCount)
                {
                    Object = Object.Duplicate();
                    Object.Count = Count;
                }
            }

            if (To != null)
            {
                if (To.AddChild(Object, List) == AddResult.Stacked)
                {
                    var StringRepresentation = Object.ParameterString();
                    var StackedObject = To.GetContents(List).Find(
                        (A) => { return A.Instance && A.ID == Object.ID && A.ParameterString() == StringRepresentation; });
                    if (StackedObject == null) throw new InvalidProgramException();
                    return StackedObject;
                }
                else
                {
                    Object.UpdateLocation();
                    return Object;
                }
            }
            else
            {
                Object.Location = new MudObject._Location(null, "", 0);
                Object.UpdateLocation();
                return Object;
            }
        }

        public static void DiscoverTree(MudObject Object)
        {
            try
            {
                if (Object.Location != null && Object.Location.Parent != null) return;
                if (!Object.HasAttribute("LOCATION")) return;
                Int64 LocID = Convert.ToInt64(Object.GetAttribute("LOCATION", ""));
                var Location = MudObject.FromID(LocID, Object._database);
                Location.BuildTree();
            }
            catch (Exception) { }
        }

        internal MudObject UnStack(String List, int Index, int Count)
        {
            if (!Contents.ContainsKey(List.ToUpper())) throw new InvalidProgramException();
            var ObjectList = Contents[List.ToUpper()];
            var Object = ObjectList[Index];
            var ObjectCount = Object.Count;

            if (Count > ObjectCount) throw new InvalidProgramException();
            if (ObjectCount - Count == 0)
            {
                return Object;
            }
            else
            {
                var NewObject = Object.Duplicate();
                Object.SetLocalAttribute("COUNT", (ObjectCount - Count).ToString());
                NewObject.SetLocalAttribute("COUNT", Count.ToString());
                NewObject.Location = new _Location(this, List.ToUpper(), ObjectList.Count);
                ObjectList.Add(NewObject);
                UpdateContentsAttribute(List);
                return NewObject;
            }
        }
    }
}
