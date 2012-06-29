using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                PostgreSQLImplementation.DatabaseService Database = new PostgreSQLImplementation.DatabaseService();

                var ClientSource = new MudEngine.TelnetClientSource();
                var MudCore = new MudEngine.MudCore(Database, Database);
                //MudEngine.MudCore.CreateDefaultDatabase(MudCore.DatabaseService);
                ClientSource.Listen(MudCore);

                //var IRCClientSource = new MudEngine.IRCClientSource();
                //IRCClientSource.ServerHostName = "irc.afternet.org";
                //IRCClientSource.Port = 6667;
                //IRCClientSource.Nick = "JemgineMudBot";
                //IRCClientSource.Channel = "Jemgine";
                //IRCClientSource.Listen(MudCore);

                MudCore.Execute();
                MudCore.Join();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }

    }
}
