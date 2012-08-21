using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public partial class Engine
    {
        private void SetupFileFunctions()
        {
            AddFunction("open-file", "Open a file.",
                (context, arguments) =>
                {
                    var mode = ScriptObject.AsString(arguments[1]);
                    if (mode.ToUpperInvariant() == "READ")
                        return System.IO.File.OpenText(ScriptObject.AsString(arguments[0]));
                    else if (mode.ToUpperInvariant() == "WRITE")
                        return System.IO.File.CreateText(ScriptObject.AsString(arguments[0]));
                    else if (mode.ToUpperInvariant() == "APPEND")
                        return System.IO.File.AppendText(ScriptObject.AsString(arguments[0]));
                    else
                        throw new ScriptError("Invalid file mode.", null);
                },
                "string file-name", "string mode");

            AddFunction("close-file", "Close a file.",
                (context, arguments) =>
                {
                    if (arguments[0] is System.IO.StreamReader) (arguments[0] as System.IO.StreamReader).Close();
                    else if (arguments[0] is System.IO.StreamWriter) (arguments[0] as System.IO.StreamWriter).Close();
                    else throw new ScriptError("Argument is not a file.", null);
                    return null;
                },
                "file");

            AddFunction("file-read-line", "Read a line from a file.",
                (context, arguments) =>
                {
                    var file = arguments[0] as System.IO.StreamReader;
                    if (file == null) throw new ScriptError("Argument is not a read file.", null);
                    return file.ReadLine();
                },
                "file");

            AddFunction("file-read-all", "Read all of a file.",
                (context, arguments) =>
                {
                    var file = arguments[0] as System.IO.StreamReader;
                    if (file == null) throw new ScriptError("Argument is not a read file.", null);
                    return file.ReadToEnd();
                },
                "file");

            AddFunction("file-write", "Write to a file.",
                (context, arguments) =>
                {
                    var file = arguments[0] as System.IO.StreamWriter;
                    if (file == null) throw new ScriptError("Argument is not a write file.", null);
                    file.Write(ScriptObject.AsString(arguments[1]));
                    return null;
                },
                "file", "string text");

            AddFunction("file-more", "Is there more to read in this file?",
                (context, arguments) =>
                {
                    var file = arguments[0] as System.IO.StreamReader;
                    if (file == null) throw new ScriptError("Argument is not a read file.", null);
                    if (file.EndOfStream) return null;
                    return true;
                },
                "file");
            
        }
    }
}
