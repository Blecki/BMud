using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012.MISP
{
    public partial class Engine
    {
        private void SetupStandardLibrary()
        {
            specialVariables.Add("null", (c, s) => { return null; });
            specialVariables.Add("this", (c, s) => { return s; });
            specialVariables.Add("functions", (c, s) => { return new ScriptList(functions.Select((pair) => { return pair.Value; })); });
            specialVariables.Add("true", (c, s) => { return true; });

            functions.Add("eval", new Function("eval", 
                ArgumentInfo.ParseArguments("object this", "code code"),
                "thisobject code : Execute code.", (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var _this = ArgumentType<ScriptObject>(arguments[0]);
                    if (arguments[1] is ParseNode)
                        return Evaluate(context, arguments[1] as ParseNode, _this, true);
                    else
                        return EvaluateString(context, _this, ScriptObject.AsString(arguments[1]));
                }));

            functions.Add("lastarg", new Function("lastarg", 
                ArgumentInfo.ParseArguments("+children"),
                "<n> : Returns the last argument.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCountOrGreater(1, arguments);
                    return arguments[arguments.Count - 1];
                }));

            functions.Add("nop", new Function("nop",
                ArgumentInfo.ParseArguments("?+value"),
                "<n> : Returns null.",
                (context, thisObject, arguments) => { return null; }));


            functions.Add("coalesce", new Function("coalesce",
                ArgumentInfo.ParseArguments("value", "default"),
                "A B : B if A is null, A otherwise.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    if (arguments[0] == null) return arguments[1];
                    return arguments[0];
                }));


            SetupVariableFunctions();
            SetupObjectFunctions();
            SetupMathFunctions();
            SetupFunctionFunctions();
            SetupBranchingFunctions();
            SetupLoopFunctions();
            SetupListFunctions();
            SetupStringFunctions();
            SetupEncryptionFunctions();
        }

    }
}
