using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class Database
    {
        public String staticPath { get; private set; }
        public String serializedPath { get; private set; }
        private Dictionary<String, MISP.ScriptObject> namedObjects = new Dictionary<string, MISP.ScriptObject>();
        private MudCore core = null;

        public Database(String basePath, MudCore core)
        {
            this.staticPath = basePath + "static/";
            this.serializedPath = basePath + "serialized/";
            this.core = core;
        }

        public MISP.ScriptObject CreateObject(String path)
        {
            if (LoadObject(path) != null) return null;
            namedObjects.Upsert(path, new MISP.GenericScriptObject("@path", path, "@base", LoadObject("object")));
            return namedObjects[path];
        }

        public MISP.ScriptObject LoadObject(String path, bool timeOut = true)
        {
            if (namedObjects.ContainsKey(path)) return namedObjects[path];
            return ReLoadObject(path);
        }

        private static int loadDepth = 0;

        public MISP.ScriptObject ReLoadObject(String path, bool timeOut = true)
        {
            if (!namedObjects.ContainsKey(path))
                namedObjects.Upsert(path, new MISP.GenericScriptObject("@path", path));
            try
            {
                Console.WriteLine(new String(' ', loadDepth) + "Loading object " + staticPath + path + ".");
                loadDepth += 1;
                bool hasData = false;

                var mudObject = namedObjects[path];
                mudObject.ClearProperties();

                var serializedObjectPath = serializedPath + path + ".obj";
                if (System.IO.File.Exists(serializedObjectPath))
                {
                    hasData = true;
                    Console.WriteLine(new String(' ', loadDepth) + "-Has serialized data.");
                    try
                    {
                        var bytes = System.IO.File.ReadAllBytes(serializedObjectPath);
                        var datagram = new ReadOnlyDatagram(bytes);
                        ObjectDeserializer.Deserialize(mudObject as MISP.GenericScriptObject, datagram, this);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: Serialized data will be discarded.\n" + e.Message);
                        mudObject.ClearProperties();
                    }
                }

                mudObject.SetProperty("@path", path);
                if (path != "object") mudObject.SetProperty("@base", LoadObject("object"));

                var staticObjectPath = staticPath + path + ".mud";
                if (System.IO.File.Exists(staticObjectPath))
                {
                    hasData = true;
                    Console.WriteLine(new String(' ', loadDepth) + "-Has static data.");
                    var inFile = System.IO.File.ReadAllText(staticObjectPath);
                    var scriptContext = new MISP.Context();
                    scriptContext.limitExecutionTime = timeOut;
                    core.scriptEngine.EvaluateString(scriptContext, mudObject, inFile, staticObjectPath, true);
                }

                if (!hasData)
                {
                    Console.WriteLine(new String(' ', loadDepth) + "-Has no data.");
                    namedObjects.Remove(path);
                    mudObject = null;
                }

                loadDepth -= 1;
                Console.WriteLine(new String(' ', loadDepth) + (hasData ? "..Success." : "..Failure."));
                return mudObject;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading object " + staticPath + path + ".");
                if (e is MISP.ScriptError)
                {
                    var node = (e as MISP.ScriptError).generatedAt;
                    if (node != null)
                    {
                        Console.Write(node.source.filename + " " + node.line + " ");
                    }
                }
                Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                loadDepth -= 1;
                return null;
            }
        }

        public void SerializeObject(String path)
        {
            if (!namedObjects.ContainsKey(path)) throw new MISP.ScriptError("Attempted to save unknown object.", null);
            var datagram = ObjectSerializer.Serialize(namedObjects[path]);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(serializedPath + path + ".obj"));
            var file = System.IO.File.OpenWrite(serializedPath + path + ".obj");
            file.Write(datagram.BufferAsArray, 0, datagram.LengthInBytes);
            file.Close();
        }

        public MISP.ScriptList EnumerateDirectory(String path)
        {
            var r = new List<String>();
            foreach (var entry in namedObjects)
                if (entry.Key.StartsWith(path)) r.Add(entry.Key);

            try
            {
                foreach (var file in System.IO.Directory.EnumerateFiles(staticPath + path))
                    r.Add(path + "/" + System.IO.Path.GetFileNameWithoutExtension(file));
            }
            catch (Exception e) { }

                        try
            {

            foreach (var file in System.IO.Directory.EnumerateFiles(serializedPath + path))
                r.Add(path + "/" + System.IO.Path.GetFileNameWithoutExtension(file));
            }
                        catch (Exception e) { }

            return new MISP.ScriptList(r.Distinct());
        }
    }
}
