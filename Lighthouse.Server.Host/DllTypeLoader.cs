using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Lighthouse.Server.Host
{
    public class DllTypeLoader
    {
        public DllTypeLoader()
        {
        }

        public IEnumerable<T> Load<T>(string folder)
            where T : class
        {
            IList<T> vals = new List<T>();
            var files = Directory.GetFiles(folder, "*.dll");
            foreach (var file in files)
            {
                
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    var allRootTypes = assembly.ExportedTypes.Where(typeinfo => typeof(T).IsAssignableFrom(typeinfo)).ToList();

                    foreach (var type in allRootTypes)
                    {
                        var package = Activator.CreateInstance(type) as T;
                        vals.Add(package);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("failed to load {0}", file);
                    Console.WriteLine(ex.ToString());
                }
            }

            return vals;

        }
    }
}