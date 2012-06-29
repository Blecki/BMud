using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MudEngine
{
    public partial class IRCClientSource : IClientSource
    {
        private class IRCClient : Client
        {
            IRCClientSource Source;
            internal String Nick;

            public IRCClient(IRCClientSource Source, String Nick)
            {
                this.Source = Source;
                this.Nick = Nick;
            }

            public override void Send(string message)
            {
                var Parts = message.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var Part in Parts)
                    Source.Send("PRIVMSG " + Nick + " :" + Part + "\n");
            }
        }

        public static byte[] STB(String _string)
        {
            var R = new byte[_string.Length];
            for (int i = 0; i < _string.Length; ++i) R[i] = (byte)_string[i];
            return R;
        }

        public static String BTS(byte[] buffer, int length)
        {
            String R = "";
            for (int i = 0; i < length; ++i) R += (char)buffer[i];
            return R;
        }

        public String ServerHostName;
        public int Port;

        public String Nick;
        public String Channel;

        Socket ServerConnection;
        MudCore Core;

        byte[] Buffer = new byte[1024];
        String Message = "";

        System.Threading.Mutex SendMutex = new System.Threading.Mutex();
        LinkedList<String> SendQueue = new LinkedList<string>();

        System.Threading.Mutex ClientLock = new System.Threading.Mutex();
        Dictionary<String, IRCClient> Clients = new Dictionary<string, IRCClient>();

        bool Registered = false;

        public void Send(String msg)
        {
            SendMutex.WaitOne();
            SendQueue.AddLast(msg);
            SendMutex.ReleaseMutex();
        }

        void DriverThread()
        {
            //var ServerConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP); ServerConnection.Connect("portivy.dyndns.org", 8669);
            ServerConnection = new Socket(AddressFamily.InterNetwork,
               SocketType.Stream, ProtocolType.IP);
            ServerConnection.Connect(ServerHostName, Port);

            ServerConnection.BeginReceive(Buffer, 0, 1024, SocketFlags.Partial, OnData, null);
            Send("NICK JMudBot\n");
            Send("USER JMudBot JMudBot@ip68-110-253-95.dc.dc.cox.net www.omnisu.com :JMudBot\n");

            while (ServerConnection.Connected)
            {
                SendMutex.WaitOne();
                if (SendQueue.Count >= 1)
                {
                    ServerConnection.Send(STB(SendQueue.First.Value));
                    Console.WriteLine("*"+SendQueue.First.Value.Substring(0, SendQueue.First.Value.Length - 1));
                    SendQueue.RemoveFirst();
                    System.Threading.Thread.Sleep(100);
                }
                SendMutex.ReleaseMutex();
            }

        }


        public void Listen(MudCore Core)
        {
            this.Core = Core;

            var _DriverThread = new System.Threading.Thread(DriverThread);
            _DriverThread.Start();
            //ServerConnection.Send(STB("/join " + Channel));
             
            
            
        }

        void OnData(IAsyncResult asyncResult)
        {
            int DataSize = ServerConnection.EndReceive(asyncResult);
            if (DataSize == 0)
            {
                ServerConnection.Close();

            }
            else
            {
                System.Console.WriteLine(BTS(Buffer, DataSize - 1));

                for (int i = 0; i < DataSize; ++i)
                {
                    char c = (char)Buffer[i];
                    if (c == '\n' || c == '\r')
                    {
                        ProcessServerMessage(Message);
                        Message = "";
                    }
                    else
                        Message += (char)Buffer[i];
                }

                ServerConnection.BeginReceive(Buffer, 0, 1024, SocketFlags.Partial, OnData, null);
            }
        }


        void ProcessServerMessage(String Message)
        {
            var Tokens = ParseMessage(Message);
            if (Tokens.Count == 0) return;

            if (Tokens[0] == "PING")
            {
                Send("PONG :" + Tokens[1] + "\n");
                if (!Registered) Send("JOIN #jemgine\n");
                Registered = true;
                return;
            }

            if (Tokens[1] == "PRIVMSG" && Tokens[2] == "JMudBot")
            {
                String Nick, Hostmask;
                ParseHostMask(Tokens[0], out Nick, out Hostmask);

                ClientLock.WaitOne();
                if (Clients.ContainsKey(Hostmask))
                {
                    var Client = Clients[Hostmask];
                    Core.ClientCommand(Client, Tokens[3]);
                }
                else
                {
                    var NewClient = new IRCClient(this, Nick);
                    Clients.Add(Hostmask, NewClient);
                    Core.NewClientConnected(NewClient);
                    Core.ClientCommand(NewClient, Tokens[3]);
                }
                ClientLock.ReleaseMutex();
            }

            if (Tokens[1] == "NICK")
            {
                String Nick, Hostmask;
                ParseHostMask(Tokens[0], out Nick, out Hostmask);

                ClientLock.WaitOne();
                if (Clients.ContainsKey(Hostmask))
                    Clients[Hostmask].Nick = Tokens[2];
                ClientLock.ReleaseMutex();
            }
        }
    }
}
