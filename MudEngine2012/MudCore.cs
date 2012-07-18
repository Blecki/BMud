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

    internal class ClientCommand : PendingAction
    {
        internal Client client;
        internal String command;

        public override void Execute(MudCore core)
        {
            try
            {
                var tokens = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var words = new MISP.ScriptList(tokens);
                if (!core.InvokeSystem(
                    client,
                    "handle_client_command", 
                    new MISP.ScriptList(new object[] { client, words }), 
                    new MISP.ScriptContext()))
                    core.SendMessage(client, "No command handler registered.\n", true);
                if (client.logged_on) core.ConnectedClients.Add(client.player.GetProperty("@path").ToString(), client);
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
            core.InvokeSystem(client, invoke, new MISP.ScriptList(new object[] { client }), new MISP.ScriptContext());
        }
    }

    public partial class MudCore
    {
        Mutex _commandLock = new Mutex();
        LinkedList<PendingAction> PendingActions = new LinkedList<PendingAction>();
        Thread ActionExecutionThread;
        public Database database { get; private set; }
        public MISP.ScriptEvaluater scriptEngine { get; private set; }
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
            Console.WriteLine("Lost client " + (client.logged_on ? client.player.GetProperty("@path").ToString() : "null") + "\n");
            if (client.logged_on)
            {
                _databaseLock.WaitOne();
                ConnectedClients.Remove(client.player.GetProperty("@path").ToString());
                _databaseLock.ReleaseMutex();
            }
            EnqueuAction(new InvokeAction(client, "handle_lost_client"));
        }

        public bool Start(String basePath)
        {
            try
            {
            scriptEngine = new MISP.ScriptEvaluater(this);
                SetupScript();
                database = new Database(basePath, this);
                database.LoadObject("system");

                ActionExecutionThread = new Thread(CommandProcessingThread);
                ActionExecutionThread.Start();

                Console.WriteLine("Engine ready with path " + basePath + ".");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start mud engine.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw e;
                return false;
            }
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

        public void SendMessage(MISP.ScriptObject Object, String _message, bool Immediate)
        {
            _databaseLock.WaitOne();
            Client client = null;

            if (Object is Client) client = Object as Client;
            else
            {
                var path = Object.GetLocalProperty("@path") as String;
                if (path != null && ConnectedClients.ContainsKey(path))
                    client = ConnectedClients[path];
            }

            if (client != null)
            {
                //Process message formatting.
                var realMessage = new StringBuilder();
                int p = 0;
                while (true)
                {
                    if (p >= _message.Length) break;
                    if (_message[p] == '\\')
                    {
                        ++p;
                        if (p >= _message.Length) break;
                        if (_message[p] == 'n') realMessage.Append("\n\r");
                        else if (_message[p] == 't') realMessage.Append("\t");
                        else realMessage.Append(_message[p]);
                    }
                    else if (_message[p] == '^')
                    {
                        ++p;
                        if (p >= _message.Length) break;
                        realMessage.Append((new String(_message[p], 1)).ToUpperInvariant());
                    }
                    else
                        realMessage.Append(_message[p]);

                    ++p;
                }

                if (Immediate) client.Send(realMessage.ToString());
                else PendingMessages.Add(new Message { _client = client, _message = realMessage.ToString() });
            }
            _databaseLock.ReleaseMutex();
        }

        public bool IsPlayerConnected(String path)
        {
            _databaseLock.WaitOne();
            bool R = ConnectedClients.ContainsKey(path);
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

        internal bool InvokeSystem(
            MISP.ScriptObject executor, 
            String property, 
            MISP.ScriptList arguments, 
            MISP.ScriptContext context)
        {
            var system = database.LoadObject("system") as MISP.ScriptObject;
            var prop = system.GetProperty(property);
            if (prop is MISP.ScriptFunction)
            {
                try
                {
                    (prop as MISP.ScriptFunction).Invoke(context, system, arguments);
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
