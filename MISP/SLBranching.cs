using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public partial class Engine
    {
        private void SetupBranchingFunctions()
        {
            Func<Context, ScriptObject, ScriptList, Object> equalBody =
                (context, thisObject, arguments) =>
                {
                    if (arguments.Count == 0) return null;

                    var nullCount = arguments.Count((o) => { return o == null; });
                    if (nullCount == arguments.Count) return true;
                    if (nullCount > 0) return null;

                    var firstType = arguments[0].GetType();
                    bool allSameType = true;
                    foreach (var argument in arguments)
                        if (argument.GetType() != firstType) allSameType = false;
                    if (!allSameType) return null;
                    for (int i = 1; i < arguments.Count; ++i)
                    {
                        if (firstType == typeof(String))
                        {
                            if (String.Compare(arguments[i] as String, arguments[i - 1] as String,
                                StringComparison.InvariantCultureIgnoreCase) != 0) return null;
                            //if (arguments[i] as String != arguments[i - 1] as String) return null;
                        }
                        else if (firstType == typeof(int))
                        {
                            if ((arguments[i] as int?).Value != (arguments[i - 1] as int?).Value) return null;
                        }
                        else if (firstType == typeof(bool))
                        {
                            if ((arguments[i] as bool?).Value != (arguments[i - 1] as bool?).Value) return null;
                        }
                        else if (!Object.ReferenceEquals(arguments[i], arguments[i - 1])) return null;
                    }
                    return true;
                };

            functions.Add("equal", new Function("equal",
                ArgumentInfo.ParseArguments("+value"),
                "<n> : True if all arguments equal, null otherwise.", equalBody));

            functions.Add("notequal", new Function("notequal",
                ArgumentInfo.ParseArguments("+value"),
                "<n> : Null if all arguments equal, true otherwise.",
                (context, thisObject, arguments) =>
                {
                    if (equalBody(context, thisObject, arguments) == null) return true;
                    return null;
                }));

            functions.Add("and", new Function("and",
                ArgumentInfo.ParseArguments("+value"),
                "<n> : True if all arguments true.",
                (context, thisObject, arguments) =>
                {
                    foreach (var arg in arguments) if (arg == null) return null;
                    return true;
                }));


            functions.Add("atleast", new Function("atleast",
                ArgumentInfo.ParseArguments("integer A", "integer B"),
                "A B : true if A >= B, null otherwise.",
                (context, thisObject, arguments) =>
                {
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    if (first.Value >= second.Value) return true;
                    return null;
                }));

            functions.Add("greaterthan", new Function("greaterthan",
                ArgumentInfo.ParseArguments("integer A", "integer B"),
                "A B : true if A > B, null otherwise.",
                (context, thisObject, arguments) =>
                {
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    if (first.Value > second.Value) return true;
                    return null;
                }));

            functions.Add("nomorethan", new Function("nomorethan",
                ArgumentInfo.ParseArguments("integer A", "integer B"),
                "A B : true if A <= B, null otherwise.",
                (context, thisObject, arguments) =>
                {
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    if (first.Value <= second.Value) return true;
                    return null;
                }));

            functions.Add("lessthan", new Function("lessthan",
                ArgumentInfo.ParseArguments("integer A", "integer B"),
                "A B : true if A < B, null otherwise.",
                (context, thisObject, arguments) =>
                {
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    if (first.Value < second.Value) return true;
                    return null;
                }));

            functions.Add("not", new Function("not",
                ArgumentInfo.ParseArguments("value"),
                "A : true if A is null, null otherwise.",
                (context, thisObject, arguments) =>
                {
                    if (arguments[0] == null) return true;
                    else return null;
                }));

            functions.Add("if", new Function("if",
                ArgumentInfo.ParseArguments("condition", "code then", "code ?else"),
                "condition then else : If condition evaluates to true, evaluate and return then. Otherwise, evaluate and return else.",
                (context, thisObject, arguments) =>
                {
                    if (arguments[0] != null)
                        return Evaluate(context, arguments[1] as ParseNode, thisObject, true);
                    else if (arguments.Count == 3)
                        return Evaluate(context, arguments[2] as ParseNode, thisObject, true);
                    return null;
                }));

        }
    }
}
