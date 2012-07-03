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

            scriptEngine.specialVariables.Add("system", (s, t) => { return database.LoadObject("system"); });

            #region Object Declaration Functions
            

            scriptEngine.functions.Add("decor", new ScriptFunction("decor", "code : Create an anonymous decorative object. Executes [code] to initialize object.", (context, thisObject, arguments) =>
            {
                var result = new GenericScriptObject();
                var code = ScriptEvaluater.ArgumentType<Irony.Parsing.ParseTreeNode>(arguments[0]);
                scriptEngine.Evaluate(context, code, result, true);
                result.SetProperty("location", thisObject);
                return result;
            }));

            scriptEngine.functions.Add("load", new ScriptFunction("load", "name : Loads an object from the database.",
                (context, thisObject, arguments) =>
                {
                    ScriptEvaluater.ArgumentCount(1, arguments);
                    try
                    {
                        return database.LoadObject(arguments[0].ToString());
                    }
                    catch (Exception e)
                    {
                        SendMessage(thisObject as MudObject, "Failed to load object " + arguments[0].ToString() + ": " + e.Message, true);
                        return null;
                    }
                }));

            scriptEngine.functions.Add("reload", new ScriptFunction("reload", "name : Reoads an object from the database.",
                (context, thisObject, arguments) =>
                {
                    ScriptEvaluater.ArgumentCount(1, arguments);
                    try
                    {
                        return database.ReLoadObject(arguments[0].ToString());
                    }
                    catch (Exception e)
                    {
                        SendMessage(thisObject as MudObject, "Failed to load object " + arguments[0].ToString() + ": " + e.Message, true);
                        return null;
                    }
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

            scriptEngine.functions.Add("discard_verb", new ScriptFunction("discard_verb", "name : Throw away an entire verb set.",
                 (context, thisObject, arguments) =>
                 {
                     ScriptEvaluater.ArgumentCount(1, arguments);
                     if (verbs.ContainsKey(arguments[0].ToString()))
                         verbs.Remove(arguments[0].ToString());
                     return null;
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
