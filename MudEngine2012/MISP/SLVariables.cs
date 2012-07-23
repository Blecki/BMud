using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012.MISP
{
    public partial class Engine
    {
        private void SetupVariableFunctions()
        {
            functions.Add("var", new Function("var",
                ArgumentInfo.ParseArguments("string name", "value"),
                "name value : Assign value to a variable named [name].", (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    if (specialVariables.ContainsKey(ScriptObject.AsString(arguments[0])))
                        throw new ScriptError("Can't assign to protected variable name.");
                    context.ChangeVariable(ScriptObject.AsString(arguments[0]), arguments[1]);
                    return arguments[1];
                }));

            functions.Add("let", new Function("let",
                ArgumentInfo.ParseArguments("list pairs", "code code"),
                "^( ^(\"name\" value) ^(...) ) code : Create temporary variables, run code.",
                (context, thisObject, arguments) =>
                {
                    ArgumentCount(2, arguments);
                    var variables = ArgumentType<ScriptList>(arguments[0]);
                    var code = ArgumentType<ParseNode>(arguments[1]);

                    foreach (var item in variables)
                    {
                        var def = ArgumentType<ScriptList>(item);
                        ArgumentCount(2, def);
                        var name = ArgumentType<String>(def[0]);
                        context.PushVariable(name, def[1]);
                    }

                    var result = Evaluate(context, code, thisObject, true);

                    foreach (var item in variables)
                    {
                        var def = ArgumentType<ScriptList>(item);
                        context.PopVariable(ArgumentType<String>(def[0]));
                    }

                    return result;
                }));

        }
    }
}
