using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public partial class Engine
    {
        private void SetupFunctionFunctions()
        {
            Func<Context, Object, ScriptList, Object> defunImple = (context, thisObject, arguments) =>
            {
                var functionName = ArgumentType<String>(arguments[0]);

                var argumentInfo = ArgumentInfo.ParseArguments(ArgumentType<ScriptList>(arguments[1]));

                var cVN = ArgumentType<ScriptList>(arguments[2]);
                var closedVariableNames = new List<String>(cVN.Select((o) =>
                {
                    if (!(o is String)) throw new ScriptError("Closed variable names MUST be strings.", context.currentNode);
                    return ScriptObject.AsString(o);
                }));

                var functionBody = ArgumentType<ParseNode>(arguments[3]);

                var closedValues = new ScriptList();

                foreach (var closedVariableName in closedVariableNames)
                {
                    if (!context.HasVariable(closedVariableName)) throw new ScriptError("Closed variable not found in parent scope.", context.currentNode);
                    closedValues.Add(context.GetVariable(closedVariableName));
                }

                var newFunction = new Function(functionName, argumentInfo, "Script-defined function", (c, to, a) =>
                    {
                        //try
                        //{
                        c.PushScope();
                        for (int i = 0; i < closedValues.Count; ++i)
                            c.PushVariable(closedVariableNames[i], closedValues[i]);
                        for (int i = 0; i < argumentInfo.Count; ++i)
                            c.PushVariable(argumentInfo[i].name, i >= a.Count ? null : a[i]);

                        var result = Evaluate(c, functionBody, to, true);

                        for (int i = 0; i < argumentInfo.Count; ++i)
                            c.PopVariable(argumentInfo[i].name);
                        for (int i = 0; i < closedValues.Count; ++i)
                            c.PopVariable(closedVariableNames[i]);
                        c.PopScope();

                        return result;
                        //}
                        //catch (ScriptError e)
                        //{
                        //    throw new ScriptError("[defun " + (String.IsNullOrEmpty(functionName) ? "anon" : functionName) + "] " + e.Message);
                        //}
                    });

                newFunction.shortHelp = ScriptObject.AsString(arguments[4]);
                
                //newFunction.source = sourceSpan(context.activeSource, (arguments[3] as Irony.Parsing.ParseTreeNode).Span);
                newFunction.closedValues = closedValues;
                return newFunction;
            };

            functions.Add("defun", new Function("defun",
                ArgumentInfo.ParseArguments("string name", "list arguments", "list closures", "code code", "?comment"),
                "name arguments closures code", (context, thisObject, arguments) =>
            {
                var r = defunImple(context, thisObject, arguments);
                if (!String.IsNullOrEmpty((r as Function).name)) functions.Upsert((r as Function).name, r as Function);
                return r;
            }));

            functions.Add("lambda", new Function("lambda",
                ArgumentInfo.ParseArguments("string name", "list arguments", "list closures", "code code", "?comment"),
                    "name arguments closures code : Same as defun with blank name.", (context, thisObject, arguments) =>
            {
                var r = defunImple(context, thisObject, arguments);
                (r as Function).isLambda = true;
                return r;
            }));

        }

    }
}
