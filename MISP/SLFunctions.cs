using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public partial class Engine
    {
        private static Scope CopyScope(Scope scope)
        {
            var r = new Scope();
            foreach (var v in scope.variables)
                r.variables.Add(v.Key, new ScriptList(v.Value[v.Value.Count - 1]));
            return r;
        }

        private Function defunImple(Context context, ScriptList arguments, bool addToScope)
        {
            var functionName = ArgumentType<String>(arguments[0]);

            List<ArgumentInfo> argumentInfo = null;
            try
            {
                argumentInfo = ArgumentInfo.ParseArguments(this, ArgumentType<ScriptList>(arguments[1]));
            }
            catch (ScriptError e)
            {
                context.RaiseNewError(e.Message, context.currentNode);
                return null;
            }

            var functionBody = ArgumentType<ParseNode>(arguments[2]);

            var newFunction = new Function(
                functionName,
                argumentInfo,
                "Script-defined function",
                functionBody,
                CopyScope(context.Scope));

            newFunction.shortHelp = ScriptObject.AsString(arguments[3]);

            if (addToScope) newFunction.declarationScope.PushVariable(newFunction.name, newFunction);

            return newFunction;
        }

        private void SetupFunctionFunctions()
        {
            functions.Add("defun", new Function("defun",
                ArgumentInfo.ParseArguments(this, "string name", "list arguments", "code code", "?comment"),
                "name arguments closures code", (context, arguments) =>
            {
                var r = defunImple(context, arguments, false);
                if (context.evaluationState == EvaluationState.Normal && !String.IsNullOrEmpty((r as Function).name)) functions.Upsert((r as Function).name, r as Function);
                return r;
            }));

            functions.Add("lambda", new Function("lambda",
                ArgumentInfo.ParseArguments(this, "string name", "list arguments", "code code", "?comment"),
                    "name arguments closures code : Same as defun with blank name.", (context, arguments) =>
            {
                var r = defunImple(context, arguments, false);
                return r;
            }));

            AddFunction("lfun", "Creates a local function. This is functionally equivilent to using 'let' to store a function in a local variable.",
                (context, arguments) =>
                {
                    var r = defunImple(context, arguments, true);
                    if (context.evaluationState == EvaluationState.Normal) context.Scope.PushVariable(r.name, r);
                    return r;
                }, "string name", "list arguments", "code code", "?comment");
        }

    }
}
