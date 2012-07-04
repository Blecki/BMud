using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MudEngine2012
{
    public class Database
    {
        public String basePath { get; private set; }
        private Dictionary<String, MudObject> namedObjects = new Dictionary<string, MudObject>();
        private MudCore core = null;

        public Database(String basePath, MudCore core)
        {
            this.basePath = basePath;
            this.core = core;
        }

        public MudObject LoadObject(String path)
        {
            if (namedObjects.ContainsKey(path)) return namedObjects[path];
            return ReLoadObject(path);
        }

        public MudObject ReLoadObject(String path)
        {
            try
            {
                var inFile = System.IO.File.ReadAllText(basePath + path + ".mud");
                var scriptContext = new ScriptContext();
                var mudObject = new MudObject(this, path);
                core.scriptEngine.EvaluateString(scriptContext, mudObject, inFile, true);

                if (namedObjects.ContainsKey(path))
                    namedObjects[path].CopyFrom(mudObject);
                else
                    namedObjects.Upsert(path, mudObject);

                Console.WriteLine("Loaded object " + basePath + path + ".");
                return mudObject;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading object " + basePath + path + ".");
                Console.WriteLine(e.Message);
                throw e;
            }
        }
    }
}
