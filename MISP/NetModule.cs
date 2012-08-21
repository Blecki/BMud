using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public class NetModule
    {
        public virtual void BindModule(Engine engine) { }

        public static void LoadModule(Engine engine, String assemblyName, String moduleName)
        {
            var assembly = System.Reflection.Assembly.LoadFrom(assemblyName);
            if (assembly == null) throw new MISP.ScriptError("Could not load assembly " + assemblyName, null);
            var moduleType = assembly.GetType(moduleName);
            if (moduleType == null) throw new MISP.ScriptError("Could not find type " + moduleName + " in assembly " + assemblyName, null);
            var module = Activator.CreateInstance(moduleType) as NetModule;
            module.BindModule(engine);
        }
    }
}
