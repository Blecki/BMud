using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine
{
    public class Tuple<A, B, C>
    {
        public A _a;
        public B _b;
        public C _c;

        public Tuple(A _a, B _b, C _c) { this._a = _a; this._b = _b; this._c = _c; }
    }

    public partial class Evaluator
    {
        private static String CreateShortListEx(
            MudObject Actor,
            List<MudObject> Objects,
            IDatabaseService _database,
            bool DisplayIds,
            OperationLimit _operationLimit,
            bool recurse = true)
        {
            var StackedObjectList = new List<Tuple<Int32, String, MudObject>>();
            if (DisplayIds)
                foreach (var Object in Objects)
                    StackedObjectList.Add(new Tuple<Int32, String, MudObject>(Object.Count, "", Object));
            else
                foreach (var Object in Objects)
                {
                    String Short = EvaluateAttributeEx(Actor, Object, Object, "SHORT", Object.ID.ToString(), _database, _operationLimit);
                    var Exists = StackedObjectList.FirstOrDefault((A) => { return A._b == Short; });
                    if (Exists != null) Exists._a += Object.Count;
                    else StackedObjectList.Add(new Tuple<Int32, String, MudObject>(Object.Count, Short, Object));
                }

            String Output = "";
            for (int i = 0; i < StackedObjectList.Count; ++i)
            {
                var _Object = StackedObjectList[i];
                if (_Object._a > 1)
                {
                    Output += _Object._a.ToVerbal() + " ";
                    Output += Evaluator.EvaluateAttributeEx(Actor, _Object._c, _Object._c, "PLURAL", "<me:short>s", _database, _operationLimit);
                }
                else
                    Output += Evaluator.EvaluateAttributeEx(Actor, _Object._c, _Object._c, "A", "a <me:short>", _database, _operationLimit);
                if (DisplayIds && _Object._c.HasAttribute("INVISIBLE")) Output += "(invisible)";
                if (DisplayIds) Output += "(" + _Object._c.ID.ToString() + ")";

                if (recurse)
                {
                    Output += Evaluator.EvaluateStringEx(
                        Actor, _Object._c, _Object._c, "<shortlist:FALSE:me:ON: (On which there \\is \\list):>",
                        _database, _operationLimit);
                }
                
                if (i == StackedObjectList.Count - 2) Output += ", and ";
                else if (i != StackedObjectList.Count - 1) Output += ", ";
            }
            return Output;
        }

        public static String FuncShortlist(List<String> Tokens, EvaluationContext Context)
        {
            bool Recurse = true;

            if (Tokens.Count >= 2 && Tokens[1].ToUpper() == "FALSE")
            {
                Recurse = false;
                Tokens.RemoveAt(1);
            }

            if (Tokens.Count > 5 || Tokens.Count < 4) return "{Error: shortlist takes 5 arguments}";

            bool DisplayIds = Context._actor.HasAttribute("DISPLAYIDS") && MudCore.GetObjectRank(Context._actor) >= 5000;
            MudObject OwnerObject = null;
            if (Tokens[1].ToUpper() == "ACTOR") OwnerObject = Context._actor;
            if (Tokens[1].ToUpper() == "ME") OwnerObject = Context._me;
            if (Tokens[1].ToUpper() == "OBJECT") OwnerObject = Context._object;

            var RawList = OwnerObject.GetContents(Tokens[2]);
            var List = new List<MudObject>();
            foreach (var Item in RawList)
            {
                if (Item == Context._actor) continue;
                if (!DisplayIds && Item.HasAttribute("INVISIBLE")) continue;
                List.Add(Item);
            }

            if (List.Count == 0)
            {
                if (Tokens.Count == 5)
                    return Evaluator.EvaluateStringEx(Context._actor, Context._me, Context._object, Tokens[4],
                        Context._database, Context._operationLimit);
                else
                    return "";
            }
            else
            {
                String Result = "";
                int TotalItems = List.Sum((A) => { return A.Count; });
                if (TotalItems == 1)
                    Result = Tokens[3].Replace("\\is", "is");
                else
                    Result = Tokens[3].Replace("\\is", "are");

                String StrList =
                    CreateShortListEx(Context._actor, List, Context._database, DisplayIds, Context._operationLimit, Recurse);

                Result = Result.Replace("\\list", StrList);

                return Evaluator.EvaluateStringEx(Context._actor, Context._me, Context._object, Result,
                    Context._database, Context._operationLimit);
            }
        }
    }
}
