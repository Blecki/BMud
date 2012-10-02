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
                System.Console.Write(what == null ? "null" : what.ToString());
            }
            else
            {
                if (what == null)
                    System.Console.Write("null");
                else if (what is ScriptList)
                {
                    var l = what as ScriptList;
                    if (l.Count > 0)
                    {
                        System.Console.Write("list [" + l.Count + "] (\n");
                        foreach (var item in l)
                        {
                            System.Console.Write(new String('.', depth * 3 + 1));
                            PrettyPrint(item, depth + 1);
                            System.Console.Write("\n");
                        }
                        System.Console.Write(new String('.', depth * 3) + ")\n");
                    }
                    else
                        System.Console.Write("list [0] ()\n");
                }
                else if (what is ScriptObject)
                {
                    var o = what as ScriptObject;
                    System.Console.Write("object (\n");
                    foreach (var item in o.ListProperties())
                    {
                        System.Console.Write(new String('.', depth * 3 + 1) + item + ": ");
                        PrettyPrint(o.GetLocalProperty(item as String), depth + 1);
                        System.Console.Write("\n");
                    }
                    System.Console.Write(new String('.', depth * 3) + ")\n");
                }
                else System.Console.Write(what.ToString());
            }
        }

        static void Main(string[] args)
        {
            Engine mispEngine = new Engine();
            Context mispContext = new Context();
            GenericScriptObject mispObject = new GenericScriptObject();
            mispContext.limitExecutionTime = false;

            mispEngine.AddFunction("run-file", "Load and run a file.",
                (context, arguments) =>
                {
                       var text = System.IO.File.ReadAllText(ScriptObject.AsString(arguments[0]));
                        return mispEngine.EvaluateString(context, text, ScriptObject.AsString(arguments[0]), false);
                },
                "string name");

            mispEngine.AddFunction("print", "Print something.",
                (context, arguments) =>
                {
                    foreach (var item in arguments)
                        PrettyPrint(item, 0);
                    return null;
                }, "?+item");

            System.Console.Write("MISP Console 1.0\n");

            Action<String> Execute = (command) =>
            {
                try
                {
                    mispContext.ResetTimer();
                    mispContext.evaluationState = EvaluationState.Normal;
                    var result = mispEngine.EvaluateString(mispContext, command, "");
                    if (mispContext.evaluationState == EvaluationState.Normal)
                    {
                        PrettyPrint(result, 0);
                        System.Console.Write("\n");
                    }
                    else
                    {
                        System.Console.WriteLine("Error:");
                        PrettyPrint(mispContext.errorObject, 0);
                    }
                }
                catch (Exception e)
                {
                    System.Console.Write("System threw an exception: " + e.Message + "\n");
                }
            };

            if (args.Length > 0)
            {
                var invoke = "(run-file \"" + args[0] + "\")";
                System.Console.WriteLine(invoke);
                Execute(invoke);
            }

            while (true)
            {
                System.Console.Write(":>");
                var command = System.Console.ReadLine();
                if (String.IsNullOrEmpty(command)) continue;
                if (command[0] == '/')
                {
                    if (command.StartsWith("/quit")) return;
                    else System.Console.Write("I don't understand.\n");
                }
                else
                {
                    Execute(command);
                }
            }
        }
    }
}
