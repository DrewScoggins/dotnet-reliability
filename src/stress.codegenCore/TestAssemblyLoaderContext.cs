using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.IO;

namespace stress.codegenCore
{
    public class TestAssemblyLoaderContext : AssemblyLoadContext
    {
        string assmPath { get; set; }
        string [] HintPath { get; set; }

        public TestAssemblyLoaderContext(string path, string [] hintPath) : base()
        {
            assmPath = path;
            HintPath = hintPath;
        }

        public TestAssemblyLoaderContext()
        {
            assmPath = "";
            HintPath = new string[0];
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            //string assemblyPath = Path.Combine(Path.GetDirectoryName(typeof(string).Assembly.Location), assemblyName.Name + ".dll");
            Assembly assembly = null;
            foreach (string s in HintPath)
            {
                string assemblyPath = Path.Combine(s, assemblyName.Name + ".dll");
                if (File.Exists(assemblyPath))
                {
                    assembly = LoadFromAssemblyPath(assemblyPath);
                }
            }
            return assembly;
        }
    }
}
