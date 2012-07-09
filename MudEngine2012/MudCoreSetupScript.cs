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
                            return p.Value.player;
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
            

            scriptEngine.functions.Add("create", new ScriptFunction("create", "code : Create an anonymous object. Executes [code] to initialize object.", (context, thisObject, arguments) =>
            {
                var result = new MudObject(database);
                var code = ScriptEvaluater.ArgumentType<ParseNode>(arguments[0]);
                scriptEngine.Evaluate(context, code, result, true);
                return result;
            }));

            scriptEngine.functions.Add("load", new ScriptFunction("load", "name : Loads an object from the database.",
                (context, thisObject, arguments) =>
                {
                    ScriptEvaluater.ArgumentCount(1, arguments);
                    var objectName = ScriptObject.AsString(arguments[0]);
                    try
                    {
                        return database.LoadObject(objectName);
                    }
                    catch (Exception e)
                    {
                        SendMessage(thisObject as MudObject, "Failed to load object " + objectName + ": " + e.Message, true);
                        return null;
                    }
                }));

            scriptEngine.functions.Add("reload", new ScriptFunction("reload", "name : Reoads an object from the database.",
                (context, thisObject, arguments) =>
                {
                    ScriptEvaluater.ArgumentCount(1, arguments);
                    var objectName = ScriptObject.AsString(arguments[0]);
                    try
                    {
                        return database.ReLoadObject(objectName);
                    }
                    catch (Exception e)
                    {
                        SendMessage(thisObject as MudObject, "Failed to load object " + objectName + ": " + e.Message, true);
                        return null;
                    }
                }));

            #endregion

            #region Debug
            scriptEngine.functions.Add("print", new ScriptFunction("print", "object : Print to the console.", (context, thisObject, arguments) =>
                {
                    Console.WriteLine(String.Join(" ", arguments.Select((o, i) => { return ScriptObject.AsString(o); })));
                    return null;
                }));
            #endregion

            #region Command Matching

            scriptEngine.functions.Add("verb", new ScriptFunction("verb", "name matcher action : Register a verb.", (context, thisObject, arguments) =>
                {
                    ScriptEvaluater.ArgumentCount(3, arguments);
                    var name = ScriptObject.AsString(arguments[0]);
                    if (!verbs.ContainsKey(name)) verbs.Add(name, new List<Verb>());
                    List<Verb> list = verbs[name];
                    var r = new Verb
                    {
                        Matcher = ScriptEvaluater.ArgumentType<ScriptFunction>(arguments[1]),
                        Action = ScriptEvaluater.ArgumentType<ScriptFunction>(arguments[2]),
                        name = name
                    };
                    list.Add(r);
                    return r;
                }));

            scriptEngine.functions.Add("alias", new ScriptFunction("alias", "name value : Register a verb alias.",
                (context, thisObject, arguments) =>
                {
                    ScriptEvaluater.ArgumentCount(2, arguments);
                    aliases.Upsert(ScriptEvaluater.ArgumentType<String>(arguments[0]), ScriptEvaluater.ArgumentType<String>(arguments[1]));
                    return null;
                }
            ));

            scriptEngine.functions.Add("discard_verb", new ScriptFunction("discard_verb", "name : Throw away an entire verb set.",
                 (context, thisObject, arguments) =>
                 {
                     ScriptEvaluater.ArgumentCount(1, arguments);
                     var name = ScriptObject.AsString(arguments[0]);
                     if (verbs.ContainsKey(name)) verbs.Remove(name);
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
                    if (obj is MudObject) SendMessage(obj as MudObject, ScriptObject.AsString(arguments[1]), false);
                    else if (obj is Client) (obj as Client).Send(ScriptObject.AsString(arguments[1]));
                }

                return null;
            }));

            scriptEngine.functions.Add("command", new ScriptFunction("command",
                "player command : Send a command as if it came from player",
                (context, thisObject, arguments) =>
                {
                    ScriptEvaluater.ArgumentCount(2, arguments);
                    EnqueuAction(new Command
                    {
                        Executor = ScriptEvaluater.ArgumentType<MudObject>(arguments[0]),
                        _Command = ScriptObject.AsString(arguments[1])
                    });
                    return null;
                }));
            #endregion


        }
    }
}
