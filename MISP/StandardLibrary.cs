using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public partial class Engine
    {
        private void SetupStandardLibrary()
        {
            types.Add("STRING", new TypeString());
            types.Add("INTEGER", new TypeGeneric(typeof(int), true));
            types.Add("LIST", new TypeList());
            types.Add("OBJECT", new TypeGeneric(typeof(ScriptObject), false));
            types.Add("CODE", ArgumentInfo.CodeType);
            types.Add("FUNCTION", new TypeGeneric(typeof(Function), true));
            types.Add("ANYTHING", Type.Anything);
            types.Add("FLOAT", new TypeGeneric(typeof(float), true));

            specialVariables.Add("null", (c, s) => { return null; });
            specialVariables.Add("this", (c, s) => { return s; });
            specialVariables.Add("functions", (c, s) => { return new ScriptList(functions.Select((pair) => { return pair.Value; })); });
            specialVariables.Add("true", (c, s) => { return true; });
            specialVariables.Add("@scope", (c, s) => { return c.Scope; });

            functions.Add("eval", new Function("eval", 
                ArgumentInfo.ParseArguments(this, "object this", "code code"),
                "thisobject code : Execute code.", (context, thisObject, arguments) =>
                {
                    var _this = ArgumentType<ScriptObject>(arguments[0]);
                    if (arguments[1] is ParseNode)
                        return Evaluate(context, arguments[1] as ParseNode, _this, true);
                    else
                        return EvaluateString(context, _this, ScriptObject.AsString(arguments[1]), "");
                }));

            functions.Add("lastarg", new Function("lastarg",
                ArgumentInfo.ParseArguments(this, "+children"),
                "<n> : Returns the last argument.",
                (context, thisObject, arguments) =>
                {
                    var list = arguments[0] as ScriptList;
                    return list[list.Count - 1];
                }));

            functions.Add("nop", new Function("nop",
                ArgumentInfo.ParseArguments(this, "?+value"),
                "<n> : Returns null.",
                (context, thisObject, arguments) => { return null; }));


            functions.Add("coalesce", new Function("coalesce",
                ArgumentInfo.ParseArguments(this, "value", "default"),
                "A B : B if A is null, A otherwise.",
                (context, thisObject, arguments) =>
                {
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
            SetupFileFunctions();
        }

    }
}
