using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MudEngine2012.MISP;

namespace MISPConsole
{
    class Program
    {
        static void PrettyPrint(Object what, int depth)
        {
            if (depth == 2)
            {
                Console.Write(what == null ? "null" : what.ToString());
            }
            else
            {
                if (what == null)
                    Console.Write("null");
                else if (what is ScriptList)
                {
                    var l = what as ScriptList;
                    if (l.Count > 0)
                    {
                        Console.Write("list [" + l.Count + "] (\n");
                        foreach (var item in l)
                        {
                            Console.Write(new String('.', depth * 3 + 1));
                            PrettyPrint(item, depth + 1);
                            Console.Write("\n");
                        }
                        Console.Write(new String('.', depth * 3) + ")\n");
                    }
                    else
                        Console.Write("list [0] ()\n");
                }
                else if (what is ScriptObject)
                {
                    var o = what as ScriptObject;
                    Console.Write("object (\n");
                    foreach (var item in o.ListProperties())
                    {
                        Console.Write(new String('.', depth * 3 + 1) + item + ": ");
                        PrettyPrint(o.GetLocalProperty(item as String), depth + 1);
                        Console.Write("\n");
                    }
                    Console.Write(new String('.', depth * 3) + ")\n");
                }
                else Console.Write(what.ToString());
            }
        }

        static void Main(string[] args)
        {
            Engine mispEngine = new Engine();
            Context mispContext = new Context();
            GenericScriptObject mispObject = new GenericScriptObject();

            mispEngine.functions.Add("serialize", new Function("serialize",
                ArgumentInfo.ParseArguments("object obj"), 
                "Test serialize functionality. Without a database attached, this just dumps raw data to the screen.",
                (context, thisObject, arguments) =>
                {
                    var data = MudEngine2012.ObjectSerializer.Serialize(arguments[0] as ScriptObject);
                    Console.Write(ASCIIEncoding.UTF8.GetString(data.BufferAsArray));
                    return null;
                }));

            Console.Write("MISP Console 1.0\n");
            while (true)
            {
                Console.Write(":>");
                var command = Console.ReadLine();
                if (String.IsNullOrEmpty(command)) continue;
                if (command[0] == '/')
                {
                    if (command.StartsWith("/quit")) return;
                    else Console.Write("I don't understand.\n");
                }
                else
                {
                    try
                    {
                        mispContext.ResetTimer();
                        var result = mispEngine.EvaluateString(mispContext, mispObject, command);
                        PrettyPrint(result, 0);
                        Console.Write("\n");
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message + "\n");
                    }
                }
            }
        }
    }
}
