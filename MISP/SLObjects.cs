using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public partial class Engine
    {
        private void SetupObjectFunctions()
        {
            functions.Add("members", new Function("members",
                ArgumentInfo.ParseArguments("object object"),
                "object : List of names of object members.", (context, thisObject, arguments) =>
                {
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    return obj.ListProperties();
                }));

            functions.Add("record", new Function("record",
                ArgumentInfo.ParseArguments("list +?pairs"),
                "<List of key-value pairs> : Returns a new generic script object.",
                (context, thisObject, arguments) =>
                {
                    var r = (new GenericScriptObject()) as ScriptObject;
                    foreach (var item in arguments[0] as ScriptList)
                    {
                        var list = item as ScriptList;
                        if (list == null || list.Count != 2) throw new ScriptError("Record expects only pairs as arguments.", context.currentNode);
                        r.SetProperty(ScriptObject.AsString(list[0]), list[1]);
                    }
                    return r;
                }));

            functions.Add("clone", new Function("clone",
                ArgumentInfo.ParseArguments("object record", "list +?pairs"),
                "record <List of key-value pairs> : Returns a new generic script object cloned from [record]",
                (context, thisObject, arguments) =>
                {
                    var from = ArgumentType<ScriptObject>(arguments[0]);
                    var r = new GenericScriptObject(from);
                    foreach (var item in arguments[1] as ScriptList)
                    {
                        var list = item as ScriptList;
                        if (list == null || list.Count != 2) throw new ScriptError("Clone expects only pairs as arguments.", context.currentNode);
                        r.SetProperty(ScriptObject.AsString(list[0]), list[1]);
                    }
                    return r;
                }));

            functions.Add("set", new Function("set",
                ArgumentInfo.ParseArguments("object object", "string property", "value"),
                "object property value : Set the member of an object.", (context, thisObject, arguments) =>
                {
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    var vName = ScriptObject.AsString(arguments[1]);
                    obj.SetProperty(vName, arguments[2]);
                    return arguments[2];
                }));

            functions.Add("multi-set", new Function("multi-set",
                ArgumentInfo.ParseArguments("object object", "list properties"),
                "object properties: Set multiple members of an object.",
                (context, thisObject, arguments) =>
                {
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    var vars = ArgumentType<ScriptList>(arguments[1]);
                    foreach (var item in vars)
                    {
                        var l = ArgumentType<ScriptList>(item);
                        if (l == null || l.Count != 2) throw new ScriptError("Multi-set expects a list of pairs.", null);
                        obj.SetProperty(ScriptObject.AsString(l[0]), l[1]);
                    }
                    return obj;
                }));

            functions.Add("delete", new Function("delete",
                ArgumentInfo.ParseArguments("object object", "string property"),
                "object property : Deletes a property from an object.",
                (context, thisObject, arguments) =>
                {
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    var vname = ScriptObject.AsString(arguments[1]);
                    var value = obj.GetProperty(vname);
                    obj.DeleteProperty(vname);
                    return value;
                }));

        }

    }
}
