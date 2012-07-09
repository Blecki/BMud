using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudServer
{
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
                }
                
                //tree.DebugEmit(0);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    throw e;
            //}
            Console.ReadKey(true);
        }
    }
}
