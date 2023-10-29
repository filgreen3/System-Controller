using System.Collections.Generic;
using System.Reflection;
using System;

namespace Filgreen3.SystemController.Editor
{
    public static class ClassFindHelper
    {
        public static Assembly[] GetAllAssembles(this System.AppDomain aAppDomain, Func<Type, bool> compereFunc)
        {
            var result = new List<Assembly>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (IsAcceptedAssembly(assembly, compereFunc))
                {
                    result.Add(assembly);
                }
            }
            return result.ToArray();
        }

        private static bool IsAcceptedAssembly(Assembly assembly, Func<Type, bool> compere)
        {
            if (assembly.IsDynamic || assembly.Location == "")
            {
                return false;
            }
            foreach (var type in assembly.ExportedTypes)
            {
                if (compere(type))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
