using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public partial class MudCore
    {
        private void SetupScript()
        {
            scriptEngine.SetupStandardLibrary();

            scriptEngine.specialVariables.Add("players", (s, t) =>
                {
                    return new ScriptList(ConnectedClients.Select((p) =>
                        {
                            return p.Value.PlayerObject;
                        }));
                });

            scriptEngine.specialVariables.Add("verbs", (s, t) =>
                {
                    var result = new ScriptList();
                    foreach (var verb in verbs)
                        result.AddRange(verb.Value);
                    return result;
                });

            scriptEngine.specialVariables.Add("system", (s, t) => { return systemObject; });

            #region Object Declaration Functions
            scriptEngine.functions.Add("prop", new ScriptFunction("prop", "property value : Set a property on this.", (context, thisObject, arguments) =>
            {
                (thisObject as MudObject).SetAttribute(arguments[0].ToString(), arguments[1]);
                return null;
            }));

            scriptEngine.functions.Add("decor", new ScriptFunction("decor", "code : Create an anonymous decorative object. Executes [code] to initialize object.", (context, thisObject, arguments) =>
            {
                var result = new MudObject(database);
                var code = ScriptEvaluater.ArgumentType<Irony.Parsing.ParseTreeNode>(arguments[0]);
                scriptEngine.Evaluate(context, code, result, true);
                result.SetAttribute("location", thisObject);
                return result;
            }));

            #endregion

            #region Debug
            scriptEngine.functions.Add("print", new ScriptFunction("print", "object : Print to the console.", (context, thisObject, arguments) =>
                {
                    Console.WriteLine(String.Join(" ", arguments.Select((o, i) => { return o == null ? "NULL" : o.ToString(); })));
                    return null;
                }));
            #endregion

            #region Command Matching

            scriptEngine.functions.Add("verb", new ScriptFunction("verb", "name matcher action : Register a verb.", (context, thisObject, arguments) =>
                {
                    if (!verbs.ContainsKey(arguments[0].ToString())) verbs.Add(arguments[0].ToString(), new List<Verb>());
                    List<Verb> list = verbs[arguments[0].ToString()];
                    var r = new Verb
                    {
                        Matcher = arguments[1] as ScriptFunction,
                        Action = arguments[2] as ScriptFunction,
                        name = arguments[0].ToString()
                    };
                    list.Add(r);
                    return r;
                }));

           

            #endregion

            #region Basic Mudding
            scriptEngine.functions.Add("echo", new ScriptFunction("echo", "player<s> message : Send text to players", (context, thisObject, arguments) =>
            {
                ScriptEvaluater.ArgumentCount(2, arguments);
                ScriptList to = null;
                if (arguments[0] is ScriptList) to = arguments[0] as ScriptList;
                else
                {
                    to = new ScriptList();
                    to.Add(arguments[0]);
                }
                foreach (var obj in to)
                {
                    var mudObject = obj as MudObject;
                    if (mudObject != null) SendMessage(mudObject, arguments[1].ToString(), false);
                }
                return null;
            }));
            #endregion


        }
    }
}
