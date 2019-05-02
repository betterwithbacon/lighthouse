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

        public IEnumerable<Type> Load<T>(string folder, Func<Type, bool> additionalFilter = null)
            where T : class
        {
            IList<Type> vals = new List<Type>();
            var files = Directory.GetFiles(folder, "*.dll");
            foreach (var file in files)
            {   
                try
                {
                    var assembly = Assembly.LoadFile(file);
                    var allRootTypes = assembly
                        .ExportedTypes
                        .Where(typeinfo => 
                            typeof(T).IsAssignableFrom(typeinfo) && 
                            (additionalFilter?.Invoke(typeinfo) ?? true)
                        ).ToList();

                    foreach (var type in allRootTypes)
                    {   
                        vals.Add(type);
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