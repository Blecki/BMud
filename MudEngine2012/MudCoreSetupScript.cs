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
            scriptEngine.AddGlobalVariable("clients", (s) =>
                {
                    return new MISP.ScriptList(ConnectedClients);
                });

            scriptEngine.AddFunction("load", "name: Load an object from the database.",
                (context, arguments) =>
                {
                    var objectName = MISP.ScriptObject.AsString(arguments[0]);
                    try
                    {
                        return database.LoadObject(objectName);
                    }
                    catch (Exception e)
                    {
                        //SendMessage(thisObject, "Failed to load object " + objectName + ": " + e.Message, true);
                        return null;
                    }
                },
                "string name");

            scriptEngine.AddFunction("reload", "name: Reloads an object from the database.",
                (context, arguments) =>
                {
                    var objectName = MISP.ScriptObject.AsString(arguments[0]);
                    try
                    {
                        return database.ReLoadObject(objectName);
                    }
                    catch (Exception e)
                    {
                        //SendMessage(thisObject, "Failed to load object " + objectName + ": " + e.Message, true);
                        return null;
                    }
                },
                "string name");

            scriptEngine.AddFunction("create", "name: Create a new named object in the database.",
                (context, arguments) =>
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
                },
                "string name");

            scriptEngine.AddFunction("save", "name: Save a named object.",
               (context, arguments) =>
               {
                   var objectName = MISP.ScriptObject.AsString(arguments[0]);
                   try
                   {
                       database.SerializeObject(objectName);
                   }
                   catch (Exception e) { }
                   return null;
               },
               "string name");

            scriptEngine.AddFunction("enumerate-database", "List all the database objects in a certain path.",
                (context, arguments) =>
                {
                    return database.EnumerateDirectory(arguments[0] as String);
                }, "string path");

            scriptEngine.AddFunction("echo", "player<s> message: Send text to players.",
                (context, arguments) =>
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
            },
            "to", "message");

            scriptEngine.AddFunction("command", "player command: Send a command as if it came from player.",
                (context, arguments) =>
                {
                    Client client = null;
                    foreach (var c in ConnectedClients)
                        if (c.player == arguments[0]) client = c;
                    if (client == null) client = new Client { player = arguments[0] as MISP.ScriptObject };
                    EnqueuAction(new Command(
                        client,
                        MISP.ScriptObject.AsString(arguments[1])));
                    return null;
                },
                "object player", "string command");

            scriptEngine.AddFunction("invoke", "seconds function arguments: Invoke a function in N seconds.",
                (context, arguments) =>
                {
                    var seconds = arguments[0] as int?;
                    if (seconds == null || !seconds.HasValue) seconds = 0;
                    EnqueuAction(new InvokeFunctionAction(
                        MISP.Engine.ArgumentType<MISP.Function>(arguments[1]),
                        MISP.Engine.ArgumentType<MISP.ScriptList>(arguments[2]),
                        seconds.Value));
                    return null;
                },
                "integer seconds",
                "function function",
                "list arguments");

            scriptEngine.AddFunction("disconnect", "player : Disconnect a player.",
                (context, arguments) =>
                {
                    EnqueuAction(new DisconnectAction(MISP.Engine.ArgumentType<MISP.ScriptObject>(arguments[0])));
                    return null;
                }, "object player");
        }
    }

    internal class DisconnectAction : PendingAction
    {
        private MISP.ScriptObject player;
        internal DisconnectAction(MISP.ScriptObject player)
            : base(0)
        {
            this.player = player;
        }

        public override void Execute(MudCore core)
        {
            core._databaseLock.WaitOne();
            Client client = null;

            if (player is Client) client = player as Client;
            else
            {
                var path = player.GetLocalProperty("@path") as String;
                if (path != null)
                    foreach (var c in core.ConnectedClients)
                        if (c.player.GetLocalProperty("@path") == path)
                            client = c;
            }

            if (client != null)
                client.Disconnect();

            core._databaseLock.ReleaseMutex();
        }
    }
}
