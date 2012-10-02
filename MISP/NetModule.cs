using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MISP
{
    public class NetModule
    {
        public virtual void BindModule(Engine engine) { }

        public static void LoadModule(Context context, Engine engine, String assemblyName, String moduleName)
        {
            var assembly = System.Reflection.Assembly.LoadFrom(assemblyName);
            if (assembly == null) { context.RaiseNewError("Could not load assembly " + assemblyName, null); return; }
            var moduleType = assembly.GetType(moduleName);
            if (moduleType == null) { context.RaiseNewError("Could not find type " + moduleName + " in assembly " + assemblyName, null); return; }
            var module = Activator.CreateInstance(moduleType) as NetModule;
            module.BindModule(engine);
        }
    }
}
