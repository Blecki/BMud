using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MudEngine.Commands;
using MudEngine.Matchers;

namespace MudEngine
{
    internal class Command
    {
        internal Int64 Executor;
        internal String _Command;
    }

    public partial class MudCore : IMessageService
    {
        Mutex _commandLock = new Mutex();
        LinkedList<Command> PendingCommands = new LinkedList<Command>();
        Thread CommandExecutionThread;
        Thread HeartbeatThread;

        private Dictionary<Int64, Client> ConnectedClients = new Dictionary<Int64, Client>();

        public IAccountService AccountService;
        public IDatabaseService DatabaseService;
        private Mutex _databaseLock = new Mutex();
        public CommandParser Parser = new CommandParser();

        public void ClientCommand(Client _client, String _rawCommand)
        {
            switch (_client.Status)
            {
                case ClientStatus.NewClient:
                    NewClientCommand(_client, _rawCommand);
                    break;
                case ClientStatus.LoggedOn:
                    _commandLock.WaitOne();
                    PendingCommands.AddLast(new Command
                    {
                        Executor = _client.PlayerObject,
                        _Command = _rawCommand
                    });
                    _commandLock.ReleaseMutex();
                    break;
                case ClientStatus.Disconnected:
                    break;
            }
        }

        public void Execute()
        {
            _databaseLock.WaitOne();
            try
            {
                CurrentTick = Convert.ToInt64(DatabaseService.QueryAttribute(DatabaseConstants.God, "TICK", "0"));
            }
            catch (Exception) { CurrentTick = 0; }
            _databaseLock.ReleaseMutex();

            CommandExecutionThread = new Thread(CommandProcessingThread);
            CommandExecutionThread.Start();

            HeartbeatThread = new Thread(HeartbeatProcessingThread);
            HeartbeatThread.Start();
        }

        public void Join()
        {
            CommandExecutionThread.Join();
            HeartbeatThread.Join();
        }

        internal class Message
        {
            public Client _client;
            public String _message;
        }

        private List<Message> PendingMessages = new List<Message>();

        public void SendMessage(MudObject Object, String _message)
        {
            _databaseLock.WaitOne();
            if (ConnectedClients.ContainsKey(Object.ID))
                PendingMessages.Add(new Message { _client = ConnectedClients[Object.ID], _message = _message });
            else
            {
                try
                {
                    Int64 PuppetID = Convert.ToInt64(Object.GetAttribute("PUPPET", ""));
                    if (ConnectedClients.ContainsKey(PuppetID))
                        PendingMessages.Add(new Message
                        {
                            _client = ConnectedClients[PuppetID],
                            _message =
                                "[PUPPET #" + Object.ID.ToString() + "] " + _message
                        });
                }
                catch (Exception)
                { }
            }
            _databaseLock.ReleaseMutex();
        }

        public bool IsPlayerConnected(Int64 ID)
        {
            _databaseLock.WaitOne();
            bool R = ConnectedClients.ContainsKey(ID);
            _databaseLock.ReleaseMutex();
            return R;
        }

        public void Command(Int64 Executor, String Command)
        {
            _commandLock.WaitOne();
            PendingCommands.AddLast(new Command { Executor = Executor, _Command = Command });
            _commandLock.ReleaseMutex();
        }

        public void CommandProcessingThread()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(10);

                Command PendingCommand = null;

                _commandLock.WaitOne();
                if (PendingCommands.Count >= 1)
                {
                    PendingCommand = PendingCommands.First.Value;
                    PendingCommands.RemoveFirst();
                }
                _commandLock.ReleaseMutex();

                if (PendingCommand != null)
                {
                    _databaseLock.WaitOne();

                    var CommandStartTime = DateTime.Now;
                    var Object = MudObject.FromID(PendingCommand.Executor, DatabaseService);

                    try
                    {
                        var Command = Parser.ParseCommand(PendingCommand._Command, Object, DatabaseService);
                        if (Command != null)
                            Command.Processor.Perform(Command.Match, DatabaseService, this);
                        else SendMessage(Object, "Huh?\n");

                        var CommandEndTime = DateTime.Now;

                        var DeltaTime = CommandEndTime - CommandStartTime;
                        if (Object.HasAttribute("DISPLAYIDS") && MudCore.GetObjectRank(Object) >= 5000)
                            SendMessage(Object, "[Command executed in " +
                                DeltaTime.TotalMilliseconds.ToString() + " milliseconds]\n");
                        SendMessage(Object, Evaluator.EvaluateAttribute(Object, Object, null, "PROMPT", ">", DatabaseService));

                        DatabaseService.CommitChanges();
                    }
                    catch (OperationLimitExceededException)
                    {
                        DatabaseService.DiscardChanges();
                        PendingMessages.Clear();
                        SendMessage(Object, "Operation limit exceeded while processing command.\n");
                    }
                    catch (Exception e)
                    {
                        DatabaseService.DiscardChanges();
                        PendingMessages.Clear();
                        SendMessage(Object,
                            e.Message + "\n" +
                            e.StackTrace + "\n");
                    }

                    ClearPendingMessages();



                    _databaseLock.ReleaseMutex();
                }
            }
        }

        private void ClearPendingMessages()
        {
            foreach (var Message in PendingMessages)
                Message._client.Send(Message._message);
            PendingMessages.Clear();
        }

        private Int64 CurrentTick = 0;
        public Int64 GetFutureTime(Int64 Ticks) { return CurrentTick + Ticks; }
        public Int64 GetNow() { return CurrentTick; }

        public void HeartbeatProcessingThread()
        {
            
            var LastTickTime = DateTime.Now.ToUniversalTime();

            while (true)
            {
                var CurrentTime = DateTime.Now.ToUniversalTime();
                if (CurrentTime >= LastTickTime.AddSeconds(DatabaseConstants.RealSecondsPerTick))
                {
                    LastTickTime = CurrentTime;
                    _databaseLock.WaitOne();
                    ++CurrentTick;
                    var TimerList = DatabaseService.QueryDueTimers(CurrentTick);
                    DatabaseService.ClearOldTimers(CurrentTick);
                    DatabaseService.WriteAttribute(DatabaseConstants.God, "TICK", CurrentTick.ToString());
                    DatabaseService.CommitChanges();
                    _databaseLock.ReleaseMutex();

                    foreach (var Item in TimerList)
                    {
                        _databaseLock.WaitOne();
                        var Object = MudObject.FromID(Item.ObjectID, DatabaseService);
                        if (Object != null)
                        {
                            try
                            {
                                if (Item.Attribute == "@PLAYER")
                                    TickPlayer(Object, DatabaseService, this);
                                else
                                {
                                    var Variables = new Script.VariableSet();
                                    Variables.Upsert("ME", Object);
                                    Script.Engine.ExecuteScript(Object, Object.GetAttribute(Item.Attribute, ""), Variables, DatabaseService, this);
                                }
                                DatabaseService.CommitChanges();
                            }
                            catch (Exception)
                            {
                                PendingMessages.Clear();
                                DatabaseService.DiscardChanges();
                            }

                            ClearPendingMessages();
                        }
                        _databaseLock.ReleaseMutex();
                        Thread.Sleep(0);
                    }
                }
                else
                    Thread.Sleep(10);
            }
        }
    }
}
