using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace MudServer
{
    class WebsocketClient : MudEngine2012.Client
    {
        internal Alchemy.Classes.UserContext context;
        public override void ImplementSend(string message)
        {
            if (context != null && context.DataFrame != null) context.Send(message);
        }

        public override void Disconnect()
        {
            //context.dis
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
          
            //try
            //{
                //var code = "(defun \"string expression   (stuff stuff)\" \"remove\" ^(\"what\" \"list\") ^() *(where \"item\" (coalesce list ^()) *(not (equal item what))))";
                //var tree = MudEngine2012.ScriptParser.ParseRoot(code);
                var mudCore = new MudEngine2012.MudCore();
                if (mudCore.Start("database/"))
                {
                    var telnetListener = new TelnetClientSource();
                    telnetListener.Listen(mudCore);

                    var websocketListener = new Alchemy.WebSocketServer(8670, IPAddress.Any);
                    websocketListener.TimeOut = TimeSpan.FromMinutes(10);


                    websocketListener.OnConnected = (context) =>
                        {
                            var client = new WebsocketClient();
                            client.context = context;
                            context.Data = client;
                            mudCore.ClientConnected(client);
                            Console.WriteLine("New Websocket client.");
                        };

                    websocketListener.OnReceive = (context) =>
                        {
                            Console.WriteLine("Data from websocket.");
                            if (context._context.Connected)
                            {
                                var data = context.DataFrame.AsRaw();
                                var stringWriter = new StringWriter();
                                foreach (var item in data)
                                    foreach (var letter in item.Array)
                                        stringWriter.Write((char)letter);
                                mudCore.ClientCommand((context.Data as MudEngine2012.Client), stringWriter.ToString());
                            }
                            else
                                Console.WriteLine("..bad client!");
                            // mudCore.ClientCommand((context.Data as MudEngine2012.Client),
                            //   context.DataFrame.
                        };

                    websocketListener.OnDisconnect = (context) =>
                        {
                            var client = context.Data as WebsocketClient;
                            if (client != null)
                            {
                                client.context = null;
                                context.Data = null;
                                Console.WriteLine("Lost websocket client.");
                                mudCore.ClientDisconnected(client);
                            }
                            else
                                Console.WriteLine("Dafuq?");
                        };
                        

                    websocketListener.Start();
                    Console.WriteLine("Accepting websocket connections on port 8670.");
                    //var websocketListener = new WebsocketClientSource();
                    //websocketListener.Listen(mudCore);
                }
                
                //tree.DebugEmit(0);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    throw e;
            //}
            //Console.ReadKey(true);
        }
    }
}
