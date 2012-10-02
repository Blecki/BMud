using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MISP;

namespace MISP
{
    public class Console
    {
        public Action<String> Write = (s) => { };
        public Engine mispEngine { get; private set; }
        public Context mispContext { get; private set; }
        public GenericScriptObject mispObject { get; private set; }

        public void PrettyPrint(Object what, int depth)
        {
            if (depth == 2)
            {
                Write(what == null ? "null" : what.ToString());
            }
            else
            {
                if (what == null)
                    Write("null");
                else if (what is ScriptList)
                {
                    var l = what as ScriptList;
                    if (l.Count > 0)
                    {
                        Write("list [" + l.Count + "] (\n");
                        foreach (var item in l)
                        {
                            Write(new String('.', depth * 3 + 1));
                            PrettyPrint(item, depth + 1);
                            Write("\n");
                        }
                        Write(new String('.', depth * 3) + ")\n");
                    }
                    else
                        Write("list [0] ()\n");
                }
                else if (what is ScriptObject)
                {
                    var o = what as ScriptObject;
                    Write("object (\n");
                    foreach (var item in o.ListProperties())
                    {
                        Write(new String('.', depth * 3 + 1) + item + ": ");
                        PrettyPrint(o.GetLocalProperty(item as String), depth + 1);
                        Write("\n");
                    }
                    Write(new String('.', depth * 3) + ")\n");
                }
                else Write(what.ToString());
            }
        }

        public static String PrettyPrint2(Object what, int depth)
        {
            var r = "";
            Action<String> Write = (s) => { r += s; };

            if (depth == 3)
            {
                Write(what == null ? "null" : what.ToString());
            }
            else
            {
                if (what == null)
                    Write("null");
                else if (what is ScriptList)
                {
                    var l = what as ScriptList;
                    if (l.Count > 0)
                    {
                        Write("list [" + l.Count + "] (\n");
                        foreach (var item in l)
                        {
                            Write(new String('.', depth * 3 + 1));
                            Write(PrettyPrint2(item, depth + 1));
                            Write("\n");
                        }
                        Write(new String('.', depth * 3) + ")\n");
                    }
                    else
                        Write("list [0] ()\n");
                }
                else if (what is ScriptObject)
                {
                    var o = what as ScriptObject;
                    Write("object (\n");
                    foreach (var item in o.ListProperties())
                    {
                        Write(new String('.', depth * 3 + 1) + item + ": ");
                        Write(PrettyPrint2(o.GetLocalProperty(item as String), depth + 1));
                        Write("\n");
                    }
                    Write(new String('.', depth * 3) + ")\n");
                }
                else Write(what.ToString());
            }

            return r;
        }


        public Console(Action<String> Write)
        {
            this.Write = Write;
            mispEngine = new Engine();
            mispContext = new Context();
            mispObject = new GenericScriptObject();

            Write("MISP Console 1.0\n");
        }

        public void Execute(String str)
        {
            try
            {
                mispContext.ResetTimer();
                var result = mispEngine.EvaluateString(mispContext, str, "");
                PrettyPrint(result, 0);
                Write("\n");
            }
            catch (Exception e)
            {
                Write(e.Message + "\n");
            }
        }
    }
}
