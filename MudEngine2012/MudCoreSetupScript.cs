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
                    return new MISP.ScriptList(ConnectedClients.Select((p) =>
                        {
                            return p.Value.player;
                        }));
                });

            scriptEngine.specialVariables.Add("system", (s, t) => { return database.LoadObject("system"); });

            #region Object Declaration Functions


            scriptEngine.functions.Add("load", new MISP.ScriptFunction("load", "name : Loads an object from the database.",
                (context, thisObject, arguments) =>
                {
                    MISP.ScriptEvaluater.ArgumentCount(1, arguments);
                    var objectName = MISP.ScriptObject.AsString(arguments[0]);
                    try
                    {
                        return database.LoadObject(objectName);
                    }
                    catch (Exception e)
                    {
                        SendMessage(thisObject, "Failed to load object " + objectName + ": " + e.Message, true);
                        return null;
                    }
                }));

            scriptEngine.functions.Add("reload", new MISP.ScriptFunction("reload", "name : Reoads an object from the database.",
                (context, thisObject, arguments) =>
                {
                    MISP.ScriptEvaluater.ArgumentCount(1, arguments);
                    var objectName = MISP.ScriptObject.AsString(arguments[0]);
                    try
                    {
                        return database.ReLoadObject(objectName);
                    }
                    catch (Exception e)
                    {
                        SendMessage(thisObject, "Failed to load object " + objectName + ": " + e.Message, true);
                        return null;
                    }
                }));

            #endregion

            #region Debug
            scriptEngine.functions.Add("print", new MISP.ScriptFunction("print", "object : Print to the console.", (context, thisObject, arguments) =>
                {
                    Console.WriteLine(String.Join(" ", arguments.Select((o, i) => { return MISP.ScriptObject.AsString(o); })));
                    return null;
                }));
            #endregion

            #region Basic Mudding
            scriptEngine.functions.Add("echo", new MISP.ScriptFunction("echo", "player<s> message : Send text to players", (context, thisObject, arguments) =>
            {
                MISP.ScriptEvaluater.ArgumentCount(2, arguments);
                MISP.ScriptList to = null;

                if (arguments[0] is MISP.ScriptList) to = arguments[0] as MISP.ScriptList;
                else
                {
                    to = new MISP.ScriptList();
                    to.Add(arguments[0]);
                }

                foreach (var obj in to)
                {
                    if (obj is Client) (obj as Client).Send(MISP.ScriptObject.AsString(arguments[1]));
                    else if (obj is MISP.ScriptObject) SendMessage(obj as MISP.ScriptObject, MISP.ScriptObject.AsString(arguments[1]), false);

                }

                return null;
            }));

            scriptEngine.functions.Add("command", new MISP.ScriptFunction("command",
                "player command : Send a command as if it came from player",
                (context, thisObject, arguments) =>
                {
                    MISP.ScriptEvaluater.ArgumentCount(2, arguments);
                    EnqueuAction(new Command
                    {
                        Executor = MISP.ScriptEvaluater.ArgumentType<MISP.ScriptObject>(arguments[0]),
                        _Command = MISP.ScriptObject.AsString(arguments[1])
                    });
                    return null;
                }));
            #endregion


        }
    }
}
