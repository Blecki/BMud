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
                mudCore.Start("database/setup.mud", "database/");

                var telnetListener = new MudEngine2012.TelnetClientSource();
                telnetListener.Listen(mudCore);


                //scriptEvaluater.functions.Add("prop", (context, thisObject, arguments) =>
                //    {
                //        thisObject.SetAttribute(arguments[0].ToString(), arguments[1]);
                //        return null;
                //    });

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadKey(true);
        }
    }
}
