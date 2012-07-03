using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudScriptTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var mudCore = new MudEngine2012.MudCore();
                if (mudCore.Start("database/"))
                {
                    var telnetListener = new MudEngine2012.TelnetClientSource();
                    telnetListener.Listen(mudCore);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadKey(true);
        }
    }
}
