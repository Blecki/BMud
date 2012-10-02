using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MudEngine2012
{
    internal class PendingAction
    {
        public DateTime ScheduledTime = DateTime.MinValue;

        internal PendingAction(float secondsDelay)
        {
            this.ScheduledTime = DateTime.Now + TimeSpan.FromSeconds(secondsDelay);
        }
        public virtual void Execute(MudCore core) { }
    }

    internal class InvokeSystemAction : PendingAction
    {
        internal Client client;
        internal String invoke;

        internal InvokeSystemAction(Client client, String invoke, float secondsDelay) : base(secondsDelay)
        {
            this.client = client;
            this.invoke = invoke;
        }

        public override void Execute(MudCore core)
        {
            core.InvokeSystem(client, invoke, new MISP.ScriptList(new object[] { client }), new MISP.Context());
        }
    }

    internal class InvokeFunctionAction : PendingAction
    {
        internal MISP.Function function;
        internal MISP.ScriptList arguments;

        internal InvokeFunctionAction(MISP.Function function, MISP.ScriptList arguments, float secondsDelay)
            : base(secondsDelay)
        {
            this.function = function;
            this.arguments = arguments;
        }

        public override void Execute(MudCore core)
        {
            function.Invoke(core.scriptEngine, new MISP.Context(), arguments);    
        }
    }

    public partial class MudCore
    {
        Mutex _commandLock = new Mutex();
        LinkedList<PendingAction> PendingActions = new LinkedList<PendingAction>();
        Thread ActionExecutionThread;
        public Database database { get; private set; }
        public MISP.Engine scriptEngine { get; private set; }
        internal List<Client> ConnectedClients = new List<Client>();

        internal Mutex _databaseLock = new Mutex();

        public MudCore()
        {
        }

        internal void EnqueuAction(PendingAction action)
        {
            _commandLock.WaitOne();
            if (PendingActions.Count == 0) PendingActions.AddLast(action);
            else
            {
                for (var node = PendingActions.Last; node != null; node = node.Previous)
                    if (node.Value.ScheduledTime < action.ScheduledTime)
                    {
                        PendingActions.AddAfter(node, action);
                        _commandLock.ReleaseMutex();
                        return;
                    }
                PendingActions.AddFirst(action);
            }
            _commandLock.ReleaseMutex();
        }

        public void ClientCommand(Client _client, String _rawCommand)
        {
            EnqueuAction(new Command(_client, _rawCommand));
        }

        public void ClientDisconnected(Client client)
        {
            Console.WriteLine("Lost client " + (client.logged_on ? client.player.GetProperty("@path").ToString() : "null") + "\n");
            if (client.logged_on)
            {
                _databaseLock.WaitOne();
                ConnectedClients.Remove(client);
                _databaseLock.ReleaseMutex();
            }
            EnqueuAction(new InvokeSystemAction(client, "handle-lost-client", 0.0f));
        }

        public void ClientConnected(Client client)
        {
            EnqueuAction(new InvokeSystemAction(client, "handle-new-client", 0.0f));
            ConnectedClients.Add(client);
        }

        public bool Start(String basePath)
        {
            try
            {
                scriptEngine = new MISP.Engine();
                SetupScript();
                database = new Database(basePath, this);
                database.LoadObject("system", false);

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
            if (Object == null) return;
            _databaseLock.WaitOne();
            Client client = null;

            if (Object is Client) client = Object as Client;
            else
            {
                var path = Object.GetLocalProperty("@path") as String;
                if (path != null)
                    foreach (var c in ConnectedClients)
                        if (c.player == Object) client = c;
            }

            if (client != null)
            {
                if (Immediate) client.Send(_message);
                else PendingMessages.Add(new Message { _client = client, _message = _message });
            }
            _databaseLock.ReleaseMutex();
        }

        public bool IsPlayerConnected(String path)
        {
            _databaseLock.WaitOne();
            bool R = false;
            foreach (var c in ConnectedClients)
                if (c.player.GetLocalProperty("@path") == path) R = true;
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
                    if (PendingCommand.ScheduledTime > DateTime.Now)
                        PendingCommand = null;
                    else
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
            MISP.Context context)
        {
            var system = database.LoadObject("system") as MISP.ScriptObject;
            var prop = system.GetProperty(property);
            if (prop is MISP.Function)
            {
                try
                {
                    (prop as MISP.Function).Invoke(scriptEngine, context, arguments);
                    if (context.evaluationState == MISP.EvaluationState.UnwindingError)
                    {
                        SendMessage(executor, MISP.Console.PrettyPrint2(context.errorObject, 0), true);
                        return false;
                    }
                    return true;
                }
                catch (MISP.ScriptError e)
                {
                    SendMessage(executor, (e.generatedAt == null ? "" : (e.generatedAt.source == null ? "" :
                        e.generatedAt.source.filename + " " + e.generatedAt.line)) + " " + e.Message, true);
                }
                catch (Exception e)
                {
                    SendMessage(executor, e.Message, true);
                    return false;
                }
            }
            return false;
        }

        internal Object InvokeSystemR(
            MISP.ScriptObject executor,
            String property,
            MISP.ScriptList arguments,
            MISP.Context context)
        {
            var system = database.LoadObject("system") as MISP.ScriptObject;
            var prop = system.GetProperty(property);
            if (prop is MISP.Function)
            {
                try
                {
                    var r = (prop as MISP.Function).Invoke(scriptEngine, context, arguments);
                    if (context.evaluationState == MISP.EvaluationState.UnwindingError)
                    {
                        SendMessage(executor, MISP.ScriptObject.AsString(context.errorObject), true);
                        return null;
                    }
                    return r;
                }
                catch (Exception e)
                {
                    SendMessage(executor, e.Message, true);
                    return null;
                }
            }
            return prop;
        }

        internal void SendPendingMessages()
        {
            foreach (var Message in PendingMessages)
                Message._client.Send(Message._message);
            PendingMessages.Clear();
        }

    }
}
