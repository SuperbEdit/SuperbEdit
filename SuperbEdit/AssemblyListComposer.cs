﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using SuperbEdit.Base;

namespace SuperbEdit
{
    public static class AssemblyListComposer
    {
        public static List<Assembly> loadedAssemblies;

        public static IEnumerable<Assembly> GetAssemblyList(bool pure = false)
        {
            loadedAssemblies = new List<Assembly>
            {
                Assembly.GetExecutingAssembly(),
                Assembly.LoadFrom("SuperbEdit.Base.dll")
            };

            GetAssembliesInFolder(loadedAssemblies, Folders.DefaultPackagesFolder);

            if (!Directory.Exists(Folders.UserFolder))
                Directory.CreateDirectory(Folders.UserFolder);

            if (!Directory.Exists(Folders.UserPackagesFolder))
                Directory.CreateDirectory(Folders.UserPackagesFolder);

            GetAssembliesInFolder(loadedAssemblies, Folders.UserPackagesFolder);

            return loadedAssemblies;
        }

        private static void GetAssembliesInFolder(List<Assembly> assemblies, string folder)
        {
            foreach (string assembly in Directory.GetFiles(folder, "*.dll"))
            {
                assemblies.Add(Assembly.LoadFrom(assembly));

                foreach (var subDir in Directory.GetDirectories(folder))
                {
                    if (!subDir.EndsWith("x86") && !subDir.EndsWith("x64"))
                    {
                        GetAssembliesInFolder(assemblies, subDir);
                    }
                    else
                    {
                        //Ignore the folder since it contains native DLLs
                        continue;
                    }
                }
            }
        }
    }
}