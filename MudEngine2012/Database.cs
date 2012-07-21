using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class Database
    {
        public String basePath { get; private set; }
        private Dictionary<String, MISP.ScriptObject> namedObjects = new Dictionary<string, MISP.ScriptObject>();
        private MudCore core = null;

        public Database(String basePath, MudCore core)
        {
            this.basePath = basePath;
            this.core = core;
        }

        public MISP.ScriptObject CreateObject(String path)
        {
            if (LoadObject(path) != null) return null;
            namedObjects.Upsert(path, new MISP.GenericScriptObject("@path", path));
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
                Console.WriteLine(new String(' ', loadDepth) + "Loading object " + basePath + path + ".");
                loadDepth += 1;
                var inFile = System.IO.File.ReadAllText(basePath + path + ".mud");
                var scriptContext = new MISP.ScriptContext();
                scriptContext.limitExecutionTime = timeOut;
                var mudObject = namedObjects[path];
                mudObject.ClearProperties();

                mudObject.SetProperty("@path", path);
                if (path != "object") mudObject.SetProperty("@base", LoadObject("object"));

                core.scriptEngine.EvaluateString(scriptContext, mudObject, inFile, true);

                loadDepth -= 1;
                Console.WriteLine(new String(' ', loadDepth) + "..Success.");
                return namedObjects[path];
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading object " + basePath + path + ".");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                loadDepth -= 1;
                return null;
            }
        }
    }
}
