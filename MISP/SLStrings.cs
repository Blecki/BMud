using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public partial class Engine
    {
        private void SetupStringFunctions()
        {
            functions.Add("substr", new Function("substr",
                ArgumentInfo.ParseArguments(this, "value", "integer start", "integer ?count"),
                "string start ?count: returns sub-string of string starting at start.",
                (context, arguments) =>
                {
                    var str = ScriptObject.AsString(arguments[0]);
                    var start = arguments[1] as int?;
                    if (start == null || !start.HasValue) return "";
                    int? length = arguments[2] as int?;
                    if (length == null || !length.HasValue) length = str.Length;

                    if (start.Value < 0) { length -= start; start = 0; }
                    if (start.Value >= str.Length) return new ScriptList();
                    if (length.Value <= 0) return new ScriptList();
                    if (length.Value + start.Value >= str.Length) length = str.Length - start.Value;
                    return str.Substring(start.Value, length.Value);
                }));

            functions.Add("strcat", new Function("strcat",
                ArgumentInfo.ParseArguments(this, "?+item"), "<n> : Concatenate many strings into one.",
                (context, arguments) =>
                {
                    var r = "";
                    foreach (var obj in arguments[0] as ScriptList)
                        if (obj == null) r += "null"; else r += ScriptObject.AsString(obj);
                    return r;
                }));

            functions.Add("strrepeat", new Function("strrepeat",
                ArgumentInfo.ParseArguments(this, "integer n", "string part"),
                "n part: Create a string consisting of part n times.",
                    (context, arguments) =>
                    {
                        var count = arguments[0] as int?;
                        if (count == null | !count.HasValue) throw new ScriptError("Expected int", context.currentNode);
                        var part = ScriptObject.AsString(arguments[1]);
                        var r = "";
                        for (int i = 0; i < count.Value; ++i) r += part;
                        return r;
                    }
            ));

            functions.Add("asstring", new Function("asstring",
                ArgumentInfo.ParseArguments(this, "value", "integer B"),
                "A B : convert A to a string to depth B.",
                (context, arguments) =>
                {
                    var depth = arguments[1] as int?;
                    if (depth == null || !depth.HasValue) return ScriptObject.AsString(arguments[0]);
                    else return ScriptObject.AsString(arguments[0], depth.Value);
                }));

            AddFunction("path-leaf", "Get the leaf on a path", (context, arguments) =>
                {
                    return System.IO.Path.GetFileName(arguments[0] as String);
                }, "string path");
        }
    }
}
