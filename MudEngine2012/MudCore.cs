﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MudEngine2012
{
    internal class Command
    {
        internal MudObject Executor;
        internal String _Command;
    }

    internal class Verb : ReflectionScriptObject
    {
        public ScriptFunction Matcher;
        public ScriptFunction Action;
        public String name;
    }

    public partial class MudCore
    {
        Mutex _commandLock = new Mutex();
        LinkedList<Command> PendingCommands = new LinkedList<Command>();
        Thread CommandExecutionThread;
        public Database database { get; private set; }
        public ScriptEvaluater scriptEngine { get; private set; }
        private Dictionary<String, List<Verb>> verbs = new Dictionary<string, List<Verb>>();
        private GenericScriptObject systemObject = new GenericScriptObject();
        private Dictionary<String, Client> ConnectedClients = new Dictionary<String, Client>();

        private Mutex _databaseLock = new Mutex();

        public MudCore()
        {
        }

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

        public void Start(String startupScript, String basePath)
        {
            scriptEngine = new ScriptEvaluater(this);
            SetupScript();
            database = new Database(basePath, this);
            var script = System.IO.File.ReadAllText(startupScript);
            var context = new ScriptContext();
            scriptEngine.EvaluateString(context, null, script);

            CommandExecutionThread = new Thread(CommandProcessingThread);
            CommandExecutionThread.Start();
        }

        public void Join()
        {
            CommandExecutionThread.Join();
        }

        internal class Message
        {
            public Client _client;
            public String _message;
        }

        private List<Message> PendingMessages = new List<Message>();

        public void SendMessage(MudObject Object, String _message, bool Immediate)
        {
            _databaseLock.WaitOne();
            if (ConnectedClients.ContainsKey(Object.path))
            {
                if (Immediate)
                    ConnectedClients[Object.path].Send(_message);
                else
                    PendingMessages.Add(new Message { _client = ConnectedClients[Object.path], _message = _message });
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

        public void Command(MudObject Executor, String Command)
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

                    try
                    {
                        var tokens = CommandTokenizer.FullyTokenizeCommand(PendingCommand._Command);
                        var firstWord = tokens.word;
                        tokens = tokens.next;

                        var arguments = new ScriptList();
                        var matchContext = new ScriptContext();
                        ScriptList matches = null;


                        if (verbs.ContainsKey(firstWord))
                        {
                            bool matchFound = false;
                            foreach (var verb in verbs[firstWord])
                            {
                                try
                                {
                                    matchContext.Reset(PendingCommand.Executor);
                                    matchContext.PushVariable("command", PendingCommand._Command);
                                    matchContext.PushVariable("actor", PendingCommand.Executor);
                                    matches = new ScriptList();
                                    matches.Add(new GenericScriptObject("token", tokens));
                                    arguments.Clear();
                                    arguments.Add(matches);
                                    matches = verb.Matcher.Invoke(matchContext, PendingCommand.Executor, arguments) as ScriptList;
                                }
                                catch (ScriptError e)
                                {
                                    SendMessage(PendingCommand.Executor, e.Message, true);
                                    matches = null;
                                }

                                if (matches == null || matches.Count == 0) continue;
                                if (!(matches[0] is GenericScriptObject))
                                {
                                    SendMessage(PendingCommand.Executor, "Matcher returned the wrong type.", true);
                                    continue;
                                }

                                matchFound = true;
                                arguments.Clear();
                                arguments.Add(matches[0]);
                                arguments.Add(PendingCommand.Executor);
                                verb.Action.Invoke(matchContext, PendingCommand.Executor, arguments);
                                break;
                            }

                            if (!matchFound)
                                SendMessage(PendingCommand.Executor, "No registered matchers matched.", false);
                        }
                        else
                        {
                            arguments.Clear();
                            arguments.Add(PendingCommand._Command);
                            arguments.Add(PendingCommand.Executor);
                            bool actionRan = false;
                            if (systemObject.properties.ContainsKey("on_unknown_verb") &&
                                systemObject.properties["on_unknown_verb"] is ScriptFunction)
                            {
                                try
                                {
                                    (systemObject.properties["on_unknown_verb"] as ScriptFunction).Invoke(
                                        matchContext, PendingCommand.Executor, arguments);
                                    actionRan = true;
                                }
                                catch (Exception e)
                                {
                                    SendMessage(PendingCommand.Executor, e.Message, true);
                                }
                            }
                            if (!actionRan)
                                SendMessage(PendingCommand.Executor, "I don't recognize that verb.", false);
                        }
                    }
                    catch (Exception e)
                    {
                        PendingMessages.Clear();
                        //DatabaseService.DiscardChanges();
                        SendMessage(PendingCommand.Executor,
                            e.Message + "\n" +
                            e.StackTrace + "\n", true);
                    }

                    SendPendingMessages();
                    _databaseLock.ReleaseMutex();
                }
            }
        }

        private void SendPendingMessages()
        {
            foreach (var Message in PendingMessages)
                Message._client.Send(Message._message);
            PendingMessages.Clear();
        }

    }
}