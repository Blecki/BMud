using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MISP;

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

            mispEngine.functions.Add("run-file", new Function("run-file",
                ArgumentInfo.ParseArguments("string name"),
                "Load and run a file.",
                (context, thisObject, arguments) =>
                {
                    try
                    {
                        var text = System.IO.File.ReadAllText(ScriptObject.AsString(arguments[0]));
                        return mispEngine.EvaluateString(context, thisObject, text, ScriptObject.AsString(arguments[0]), false);
                    }
                    catch (ScriptError e)
                    {
                        Console.WriteLine("Error " + (e.generatedAt == null ? "" : "on line " + e.generatedAt.line) + ": " + e.Message);
                        return null;
                    }
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
                        var result = mispEngine.EvaluateString(mispContext, mispObject, command, "");
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
