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

            specialVariables.Add("null", (c) => { return null; });
            specialVariables.Add("functions", (c) => { return new ScriptList(functions.Select((pair) => { return pair.Value; })); });
            specialVariables.Add("true", (c) => { return true; });
            specialVariables.Add("false", (c) => { return null; });
            specialVariables.Add("@scope", (c) => { return c.Scope; });

            AddFunction("net-module", "Loads a module from a .net assembly",
                (context, arguments) =>
                {
                    NetModule.LoadModule(context, this, ScriptObject.AsString(arguments[0]), ScriptObject.AsString(arguments[1]));
                    return null;
                }, "string assembly", "string module");

            functions.Add("eval", new Function("eval", 
                ArgumentInfo.ParseArguments(this, "code code"),
                "thisobject code : Execute code.", (context, arguments) =>
                {
                    if (arguments[0] is ParseNode)
                        return Evaluate(context, arguments[0] as ParseNode, true);
                    else
                        return EvaluateString(context, ScriptObject.AsString(arguments[0]), "");
                }));

            functions.Add("lastarg", new Function("lastarg",
                ArgumentInfo.ParseArguments(this, "+children"),
                "<n> : Returns the last argument.",
                (context, arguments) =>
                {
                    var list = arguments[0] as ScriptList;
                    return list[list.Count - 1];
                }));

            functions.Add("nop", new Function("nop",
                ArgumentInfo.ParseArguments(this, "?+value"),
                "<n> : Returns null.",
                (context, arguments) => { return null; }));


            functions.Add("coalesce", new Function("coalesce",
                ArgumentInfo.ParseArguments(this, "value", "default"),
                "A B : B if A is null, A otherwise.",
                (context, arguments) =>
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
