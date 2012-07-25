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
            scriptEngine.specialVariables.Add("players", (s, t) =>
                {
                    return new MISP.ScriptList(ConnectedClients.Select((p) =>
                        {
                            return p.Value.player;
                        }));
                });

            #region Object Declaration Functions
            scriptEngine.functions.Add("load", new MISP.Function("load",
                MISP.ArgumentInfo.ParseArguments("string name"),
                "name : Loads an object from the database.",
                (context, thisObject, arguments) =>
                {
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

            scriptEngine.functions.Add("reload", new MISP.Function("reload",
                MISP.ArgumentInfo.ParseArguments("string name"),
                "name : Reoads an object from the database.",
                (context, thisObject, arguments) =>
                {
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

            scriptEngine.functions.Add("create", new MISP.Function("create",
                MISP.ArgumentInfo.ParseArguments("string name"),
                "name : Creates a new object in the database.",
                (context, thisObject, arguments) =>
                {
                    var objectName = MISP.ScriptObject.AsString(arguments[0]);
                    try
                    {
                        return database.CreateObject(objectName);
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }));

            scriptEngine.functions.Add("save", new MISP.Function("save",
               MISP.ArgumentInfo.ParseArguments("string name"),
               "name : Save a named object.",
               (context, thisObject, arguments) =>
               {
                   var objectName = MISP.ScriptObject.AsString(arguments[0]);
                   try
                   {
                       database.SerializeObject(objectName);
                   }
                   catch (Exception e) { }
                   return null;
               }));

            #endregion

            #region Basic Mudding
            scriptEngine.functions.Add("echo", new MISP.Function("echo",
                MISP.ArgumentInfo.ParseArguments("to", "message"),
                "player<s> message : Send text to players", (context, thisObject, arguments) =>
            {
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

            scriptEngine.functions.Add("command", new MISP.Function("command",
                MISP.ArgumentInfo.ParseArguments("object player", "string command"),
                "player command : Send a command as if it came from player",
                (context, thisObject, arguments) =>
                {
                    EnqueuAction(new Command
                    {
                        Executor = MISP.Engine.ArgumentType<MISP.ScriptObject>(arguments[0]),
                        _Command = MISP.ScriptObject.AsString(arguments[1])
                    });
                    return null;
                }));
            #endregion


        }
    }
}
