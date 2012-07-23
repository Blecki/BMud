using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012.MISP
{
    public partial class Engine
    {
        private void SetupObjectFunctions()
        {
            functions.Add("members", new Function("members",
                ArgumentInfo.ParseArguments("object object"),
                "object : List of names of object members.", (context, thisObject, arguments) =>
                {
                    ArgumentCount(1, arguments);
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    return obj.ListProperties();
                }));

            functions.Add("record", new Function("record",
                ArgumentInfo.ParseArguments("list +?pairs"),
                "<List of key-value pairs> : Returns a new generic script object.",
                (context, thisObject, arguments) =>
                {
                    var r = (new GenericScriptObject()) as ScriptObject;
                    foreach (var item in arguments)
                    {
                        var list = item as ScriptList;
                        if (list == null || list.Count != 2) throw new ScriptError("Record expects only pairs as arguments.");
                        r.SetProperty(ScriptObject.AsString(list[0]), list[1]);
                    }
                    return r;
                }));

            functions.Add("clone", new Function("clone",
                ArgumentInfo.ParseArguments("object record", "list +?pairs"),
                "record <List of key-value pairs> : Returns a new generic script object cloned from [record]",
                (context, thisObject, arguments) =>
                {
                    ArgumentCountOrGreater(1, arguments);
                    var from = ArgumentType<ScriptObject>(arguments[0]);
                    var r = new GenericScriptObject(from);
                    foreach (var item in arguments.GetRange(1, arguments.Count - 1))
                    {
                        var list = item as ScriptList;
                        if (list == null || list.Count != 2) throw new ScriptError("Clone expects only pairs as arguments.");
                        r.SetProperty(ScriptObject.AsString(list[0]), list[1]);
                    }
                    return r;
                }));

            functions.Add("set", new Function("set",
                ArgumentInfo.ParseArguments("object object", "string property", "value"),
                "object property value : Set the member of an object.", (context, thisObject, arguments) =>
                {
                    ArgumentCount(3, arguments);
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    var vName = ScriptObject.AsString(arguments[1]);
                    obj.SetProperty(vName, arguments[2]);
                    return arguments[2];
                }));

            functions.Add("delete", new Function("delete",
                ArgumentInfo.ParseArguments("object object", "string property"),
                "object property : Deletes a property from an object.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var obj = ArgumentType<ScriptObject>(arguments[0]);
                    var vname = ScriptObject.AsString(arguments[1]);
                    var value = obj.GetProperty(vname);
                    obj.DeleteProperty(vname);
                    return value;
                }));

        }

    }
}
