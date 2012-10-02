using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public partial class Engine
    {
        private void SetupListFunctions()
        {
            functions.Add("length", new Function("length",
                ArgumentInfo.ParseArguments(this, "list"),
                "list : Returns length of list.",
                (context, arguments) =>
                {
                    var list = arguments[0] as ScriptList;
                    return list == null ? 0 : list.Count;
                }));

            functions.Add("count", new Function("count",
                ArgumentInfo.ParseArguments(this, "string variable_name", "list in", "code code"),
                "variable_name list code : Returns number of items in list for which code evaluated to true.",
                (context, arguments) =>
                {
                    var vName = ArgumentType<String>(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var func = ArgumentType<ParseNode>(arguments[2]);

                    context.Scope.PushVariable(vName, null);
                    var result = (int)list.Count((o) =>
                    {
                        context.Scope.ChangeVariable(vName, o);

                        return Evaluate(context, func, true) != null;
                    });
                    context.Scope.PopVariable(vName);
                    return result;
                }));

            functions.Add("where", new Function("where",
    ArgumentInfo.ParseArguments(this, "string variable_name", "list in", "code code"),
    "variable_name list code : Returns new list containing only the items in list for which code evaluated to true.",
    (context, arguments) =>
    {
        var vName = ArgumentType<String>(arguments[0]);
        var list = ArgumentType<ScriptList>(arguments[1]);
        var func = ArgumentType<ParseNode>(arguments[2]);

        context.Scope.PushVariable(vName, null);
        var result = new ScriptList(list.Where((o) =>
        {
            context.Scope.ChangeVariable(vName, o);
            return Evaluate(context, func, true) != null;
        }));
        context.Scope.PopVariable(vName);
        return result;
    }));

            functions.Add("cat", new Function("cat",
                ArgumentInfo.ParseArguments(this, "?+items"),
                "<n> : Combine N lists into one",
                (context, arguments) =>
                {
                    var result = new ScriptList();
                    foreach (var arg in arguments[0] as ScriptList)
                    {
                        if (arg is ScriptList) result.AddRange(arg as ScriptList);
                        else result.Add(arg);
                    }
                    return result;
                }));

            functions.Add("last", new Function("last",
                ArgumentInfo.ParseArguments(this, "list list"),
                "list : Returns last item in list.",
                (context, arguments) =>
                {
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    if (list.Count == 0) return null;
                    return list[list.Count - 1];
                }));

            functions.Add("first", new Function("first",
                ArgumentInfo.ParseArguments(this, "list list"),
                "list : Returns first item in list.",
                (context, arguments) =>
                {
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    if (list.Count == 0) return null;
                    return list[0];
                }));

            functions.Add("index", new Function("index",
                ArgumentInfo.ParseArguments(this, "list list", "integer n"),
                "list n : Returns nth element in list.",
                (context, arguments) =>
                {
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    var index = arguments[1] as int?;
                    if (index == null || !index.HasValue) return null;
                    if (index.Value < 0 || index.Value >= list.Count) return null;
                    return list[index.Value];
                }));

            functions.Add("sub-list", new Function("sub-list",
                ArgumentInfo.ParseArguments(this, "list list", "integer start", "integer ?length"),
                "list start length: Returns a elements in list between start and start+length.",
                (context, arguments) =>
                {
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    var start = arguments[1] as int?;
                    if (start == null || !start.HasValue) return new ScriptList();
                    int? length = arguments[2] as int?;
                    if (length == null || !length.HasValue) length = list.Count;

                    if (start.Value < 0) { length -= start; start = 0; }
                    if (start.Value >= list.Count) return new ScriptList();
                    if (length.Value <= 0) return new ScriptList();
                    if (length.Value + start.Value >= list.Count) length = list.Count - start.Value;

                    return new ScriptList(list.GetRange(start.Value, length.Value));
                }));

            functions.Add("sort", new Function("sort",
                ArgumentInfo.ParseArguments(this, "string variable_name", "list in", "code code"),
                "vname list sort_func: Sorts elements according to sort func; sort func returns integer used to order items.",
                (context, arguments) =>
                {
                    var vName = ScriptObject.AsString(arguments[0]);
                    var list = ArgumentType<ScriptList>(arguments[1]);
                    var sortFunc = ArgumentType<ParseNode>(arguments[2]);

                    var comparer = new ListSortComparer(this, vName, sortFunc, context);
                    list.Sort(comparer);
                    return list;
                }));

            functions.Add("reverse", new Function("reverse",
                ArgumentInfo.ParseArguments(this, "list list"),
                "list: Reverse the list.",
                (context, arguments) =>
                {
                    var list = ArgumentType<ScriptList>(arguments[0]);
                    list.Reverse();
                    return list;
                }));
        }

        private class ListSortComparer : IComparer<Object>
        {
            Engine evaluater;
            Context context;
            ParseNode func;
            String vName;

            internal ListSortComparer(Engine evaluater, String vName, ParseNode func, Context context)
            {
                this.evaluater = evaluater;
                this.vName = vName;
                this.func = func;
                this.context = context;
            }

            private int rank(Object o)
            {
                context.Scope.PushVariable(vName, o);
                var r = evaluater.Evaluate(context, func, true) as int?;
                context.Scope.PopVariable(vName);
                if (r != null && r.HasValue) return r.Value;
                return 0;
            }

            public int Compare(object x, object y)
            {
                return rank(x) - rank(y);
            }
        }
    }
}
