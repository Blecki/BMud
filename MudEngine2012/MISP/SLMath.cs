using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012.MISP
{
    public partial class Engine
    {
        private Random random = new Random();

        private void SetupMathFunctions()
        {
            functions.Add("subtract", new Function("subtract",
                ArgumentInfo.ParseArguments("integer A", "integer B"),
                "A B : return A-B.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    return first.Value - second.Value;
                }));

            functions.Add("add", new Function("add",
                ArgumentInfo.ParseArguments("integer A", "integer B"),
                "A B : return A+B.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    return first.Value + second.Value;
                }));

            functions.Add("random", new Function("random",
                ArgumentInfo.ParseArguments("integer A", "integer B"),
                "A B : return a random value in range (A,B).",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    return random.Next(first.Value, second.Value);
                }));

            functions.Add("multiply", new Function("multiply",
                ArgumentInfo.ParseArguments("integer A", "integer B"),
                "A B : return A*B.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var first = arguments[0] as int?;
                    var second = arguments[1] as int?;
                    if (first == null || second == null || !first.HasValue || !second.HasValue) return null;
                    return first.Value * second.Value;
                }));

        }
    }
}
