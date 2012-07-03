using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class StandardClient : Client
    {
        public System.Net.Sockets.Socket Socket = null;
        public String CommandQueue = "";
        public byte[] Storage = new byte[1024];

        private static byte[] SendBuffer = new byte[1024];

        override public void Send(String message)
        {
            int bytesSent = 0;

            if (!String.IsNullOrEmpty(CommandQueue)) message = "\r" + message + CommandQueue;

            message = message.Replace("\\n", "\n");
            message = message.Replace("\n", "\n\r");

            while (bytesSent < message.Length)
            {
                int thisChunk = 0;
                for (int i = bytesSent; i < message.Length && thisChunk < 1024; ++i, ++thisChunk)
                    SendBuffer[thisChunk] = (byte)message[i];
                if (Socket != null) Socket.Send(SendBuffer, thisChunk, System.Net.Sockets.SocketFlags.None);
                bytesSent += thisChunk;
            }
        }

        override public void Disconnect()
        {
            if (Socket != null) Socket.Close();
        }
    }

    public class TelnetClientSource
    {
        public int Port = 8669;

        MudCore MudCore;
        System.Net.Sockets.Socket ListenSocket = null;

        static System.Threading.Mutex ClientLock = new System.Threading.Mutex();
        static LinkedList<StandardClient> Clients = new LinkedList<StandardClient>();

        public void Listen(MudCore MudCore)
        {
            this.MudCore = MudCore;

            ListenSocket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.IP);

            ListenSocket.Bind(new System.Net.IPEndPoint(0, Port));
            ListenSocket.Listen(16);
            ListenSocket.BeginAccept(OnNewClient, null);

            Console.WriteLine("Listening on port " + Port);
        }

        void OnNewClient(IAsyncResult _asyncResult)
        {
            System.Net.Sockets.Socket ClientSocket = ListenSocket.EndAccept(_asyncResult);
            ListenSocket.BeginAccept(OnNewClient, null);

            var NewClient = new StandardClient { Socket = ClientSocket };
            //MudCore.NewClientConnected(NewClient);            
            ClientSocket.BeginReceive(NewClient.Storage, 0, 1024, System.Net.Sockets.SocketFlags.Partial, OnData, NewClient);
            Console.WriteLine("New client: " + ClientSocket.RemoteEndPoint.ToString());
        }

        private static string ValidCharacters = "@=|^\\;?:#.,!\"'$*<>/()[]{}-_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";

        void OnData(IAsyncResult _asyncResult)
        {
            var Client = _asyncResult.AsyncState as StandardClient;
            System.Net.Sockets.SocketError Error;
            int DataSize = Client.Socket.EndReceive(_asyncResult, out Error);

            if (DataSize == 0 || Error != System.Net.Sockets.SocketError.Success)
            {
                MudCore.ClientDisconnected(Client);
            }
            else
            {
                for (int i = 0; i < DataSize; ++i)
                {
                    if (Client.Storage[i] == '\n' || Client.Storage[i] == '\r')
                    {
                        if (!String.IsNullOrEmpty(Client.CommandQueue))
                        {
                            String Command = Client.CommandQueue;
                            Client.CommandQueue = "";
                            MudCore.ClientCommand(Client, Command);
                        }
                    }
                    else if (Client.Storage[i] == '\b')
                    {
                        if (Client.CommandQueue.Length > 0)
                            Client.CommandQueue = Client.CommandQueue.Remove(Client.CommandQueue.Length - 1);
                    }
                    else if (ValidCharacters.Contains((char)Client.Storage[i]))
                        Client.CommandQueue += (char)Client.Storage[i];
                }

                Client.Socket.BeginReceive(Client.Storage, 0, 1024, System.Net.Sockets.SocketFlags.Partial, OnData, Client);
            }
        }
    }
}
