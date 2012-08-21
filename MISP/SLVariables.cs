using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    internal class LetVariable
    {
        public String name;
        public Object cleanupCode;
    }

    public partial class Engine
    {
        private void SetupVariableFunctions()
        {
            functions.Add("var", new Function("var",
                ArgumentInfo.ParseArguments(this, "string name", "value"),
                "name value : Assign value to a variable named [name].", (context, arguments) =>
                {
                    if (specialVariables.ContainsKey(ScriptObject.AsString(arguments[0])))
                        throw new ScriptError("Can't assign to protected variable name.", context.currentNode);
                    context.Scope.ChangeVariable(ScriptObject.AsString(arguments[0]), arguments[1]);
                    return arguments[1];
                }));

            functions.Add("let", new Function("let",
                ArgumentInfo.ParseArguments(this, "code pairs", "code code"),
                "^( ^(\"name\" value ?cleanup-code) ^(...) ) code : Create temporary variables, run code. Optional clean-up code for each variable.",
                (context, arguments) =>
                {
                    var variables = ArgumentType<ParseNode>(arguments[0]);
                    if (variables.prefix != Prefix.None) { /* Raise warning */ }

                    var code = ArgumentType<ParseNode>(arguments[1]);
                    var cleanUp = new List<LetVariable>();

                    foreach (var item in variables.childNodes)
                    {
                        var def = ArgumentType<ScriptList>(Evaluate(context, item));
                        if (def.Count != 2 && def.Count != 3) 
                            throw new ScriptError("Variable defs to let should have only 2 or 3 items.", context.currentNode);
                        var name = ArgumentType<String>(def[0]);
                        context.Scope.PushVariable(name, def[1]);
                        cleanUp.Add(new LetVariable { name = name, cleanupCode = def.Count == 3 ? def[2] : null });
                    }

                    var result = Evaluate(context, code, true);

                    foreach (var item in cleanUp)
                    {
                        if (item.cleanupCode != null) Evaluate(context, item.cleanupCode, true, true);
                        context.Scope.PopVariable(item.name);
                    }

                    return result;
                }));           
        }
    }
}
