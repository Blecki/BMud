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

        private static int loadDepth = 0;

        public MudObject ReLoadObject(String path)
        {
            if (!namedObjects.ContainsKey(path))
                namedObjects.Upsert(path, new MudObject(this, path));
            try
            {
                Console.WriteLine(new String(' ', loadDepth) + "Loading object " + basePath + path + ".");
                loadDepth += 1;
                var inFile = System.IO.File.ReadAllText(basePath + path + ".mud");
                var scriptContext = new ScriptContext();
                var mudObject = namedObjects[path];
                mudObject.ClearProperties();

                //var root = ScriptParser.ParseRoot(inFile);
                //Console.WriteLine("Parse tree of " + path);
                //root.DebugEmit(0);

                core.scriptEngine.EvaluateString(scriptContext, mudObject, inFile, true);

                loadDepth -= 1;
                Console.WriteLine(new String(' ', loadDepth) + "..Success.");
                return namedObjects[path];
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading object " + basePath + path + ".");
                Console.WriteLine(e.Message);
                loadDepth -= 1;
                return null;
            }
        }
    }
}
