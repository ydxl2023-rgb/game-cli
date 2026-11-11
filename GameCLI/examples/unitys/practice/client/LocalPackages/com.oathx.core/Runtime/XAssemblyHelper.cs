using System.Collections.Generic;
using System.Reflection;

namespace Oathx.Core
{
    /// <summary>
    /// assembly helper
    /// </summary>
    public static class XAssemblyHelper
    {
        /// <summary>
        /// assembly list
        /// </summary>
        static List<Assembly> assemblys = new List<Assembly>();

        /// <summary>
        /// load assembly from raw bytes
        /// </summary>
        /// <param name="rawAssembly"></param>
        /// <returns></returns>
        public static Assembly Load(byte[] rawAssembly)
        {
            Assembly assembly = Assembly.Load(rawAssembly);
            if (assembly != null)
            {
                AddAssembly(assembly);
            }

            return assembly;
        }

        /// <summary>
        /// add assembly
        /// </summary>
        /// <param name="assembly"></param>
        public static void AddAssembly(Assembly assembly)
        {
            bool added = true;
            foreach (Assembly v in assemblys)
            {
                if (v.GetName() == assembly.GetName())
                {
                    added = false;
                    break;
                }
            }

            if (added)
            {
                assemblys.Add(assembly);
            }
        }

        /// <summary>
        /// get type by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static System.Type GetType(string name)
        {
            foreach (Assembly assembly in assemblys)
            {
                System.Type type = assembly.GetType(name);
                if (type != null)
                    return type;
            }

            throw new System.NullReferenceException(name);
        }
    }
}
