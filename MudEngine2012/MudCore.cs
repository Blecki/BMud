using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MudEngine2012
{
    internal class PendingAction
    {
        public virtual void Execute(MudCore core) { }
    }


    internal class Verb : ReflectionScriptObject
    {
        public ScriptFunction Matcher;
        public ScriptFunction Action;
        public String name;
        public String comment;
    }

    internal class ClientCommand : PendingAction
    {
        internal Client client;
        internal String command;

        public override void Execute(MudCore core)
        {
            try
            {
                var tokens = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var words = new ScriptList(tokens);
                if (!core.InvokeSystem(client, "handle_client_command", new ScriptList(new object[] { client, words }), new ScriptContext()))
                    core.SendMessage(client, "No command handler registered.\n", true);
                if (client.logged_on) core.ConnectedClients.Add(client.player.path, client);
            }
            catch (Exception e)
            {
                core.PendingMessages.Clear();
                core.SendMessage(client,
                    e.Message + "\n" +
                    e.StackTrace + "\n", true);
            }
        }
    }

    internal class InvokeAction : PendingAction
    {
        internal Client client;
        internal String invoke;

        internal InvokeAction(Client client, String invoke)
        {
            this.client = client;
            this.invoke = invoke;
        }

        public override void Execute(MudCore core)
        {
            core.InvokeSystem(client, invoke, new ScriptList(new object[] { client }), new ScriptContext());
        }
    }

    public partial class MudCore
    {
        Mutex _commandLock = new Mutex();
        LinkedList<PendingAction> PendingActions = new LinkedList<PendingAction>();
        Thread ActionExecutionThread;
        public Database database { get; private set; }
        public ScriptEvaluater scriptEngine { get; private set; }
        internal Dictionary<String, List<Verb>> verbs = new Dictionary<string, List<Verb>>();
        internal Dictionary<String, String> aliases = new Dictionary<string, string>();
        internal Dictionary<String, Client> ConnectedClients = new Dictionary<String, Client>();

        private Mutex _databaseLock = new Mutex();

        public MudCore()
        {
        }

        internal void EnqueuAction(PendingAction action)
        {
            _commandLock.WaitOne();
            PendingActions.AddLast(action);
            _commandLock.ReleaseMutex();
        }

        public void ClientCommand(Client _client, String _rawCommand)
        {
            if (_client.logged_on)
                EnqueuAction(new Command { Executor = _client.player, _Command = _rawCommand });
            else
                EnqueuAction(new ClientCommand { client = _client, command = _rawCommand });
        }

        public void ClientDisconnected(Client client)
        {
            Console.WriteLine("Lost client " + (client.logged_on ? client.player.path : "null") + "\n");
            if (client.logged_on)
            {
                _databaseLock.WaitOne();
                ConnectedClients.Remove(client.player.path);
                _databaseLock.ReleaseMutex();
            }
            EnqueuAction(new InvokeAction(client, "handle_lost_client"));
        }

        public bool Start(String basePath)
        {
            //try
            //{
                scriptEngine = new ScriptEvaluater(this);
                SetupScript();
                database = new Database(basePath, this);
                database.LoadObject("system");

                ActionExecutionThread = new Thread(CommandProcessingThread);
                ActionExecutionThread.Start();

                Console.WriteLine("Engine ready with path " + basePath + ".");
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Failed to start mud engine.");
            //    Console.WriteLine(e.Message);
            //    Console.WriteLine(e.StackTrace);
            //    throw e;
            //    return false;
            //}
            return true;
        }

        public void Join()
        {
            ActionExecutionThread.Join();
        }

        internal class Message
        {
            public Client _client;
            public String _message;
        }

        internal List<Message> PendingMessages = new List<Message>();

        public void SendMessage(ScriptObject Object, String _message, bool Immediate)
        {
            _databaseLock.WaitOne();
            Client client = null;
            if (Object is Client) client = Object as Client;
            else if (Object is MudObject && ConnectedClients.ContainsKey((Object as MudObject).path))
                client = ConnectedClients[(Object as MudObject).path];
            if (client != null)
            {
                if (Immediate) client.Send(_message);
                else PendingMessages.Add(new Message { _client = client, _message = _message });
            }
            _databaseLock.ReleaseMutex();
        }

        public bool IsPlayerConnected(String ID)
        {
            _databaseLock.WaitOne();
            bool R = ConnectedClients.ContainsKey(ID);
            _databaseLock.ReleaseMutex();
            return R;
        }

        public void CommandProcessingThread()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10);

                PendingAction PendingCommand = null;

                _commandLock.WaitOne();
                if (PendingActions.Count >= 1)
                {
                    PendingCommand = PendingActions.First.Value;
                    PendingActions.RemoveFirst();
                }
                _commandLock.ReleaseMutex();

                if (PendingCommand != null)
                {
                    _databaseLock.WaitOne();
                    PendingCommand.Execute(this);
                    SendPendingMessages();
                    _databaseLock.ReleaseMutex();
                }
            }
        }

        internal bool InvokeSystem(ScriptObject executor, String property, ScriptList arguments, ScriptContext context)
        {
            var system = database.LoadObject("system") as ScriptObject;
            var prop = system.GetProperty(property);
            if (prop is ScriptFunction)
            {
                try
                {
                    (prop as ScriptFunction).Invoke(context, system, arguments);
                    return true;
                }
                catch (Exception e)
                {
                    SendMessage(executor, e.Message, true);
                    return false;
                }
            }
            return false;
        }

        internal void SendPendingMessages()
        {
            foreach (var Message in PendingMessages)
                Message._client.Send(Message._message);
            PendingMessages.Clear();
        }

    }
}
