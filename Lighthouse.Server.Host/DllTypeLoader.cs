using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Lighthouse.Server.Host
{
    public static class DllTypeLoader
    {
        public static IEnumerable<Type> Load<T>(string folder, Func<Type, bool> additionalFilter = null)
            where T : class
        {
            List<Type> vals = new List<Type>();
            var files = Directory.GetFiles(folder, "*.dll");

            var targetName = typeof(T).FullName;
            foreach (var file in files)
            {   
                try
                {
                    var assembly = Assembly.LoadFile(file);                    
                    var allRootTypes = assembly
                        .ExportedTypes
                        .Where(typeinfo =>
                            typeinfo.GetInterfaces().Select(t => t.FullName).Contains(targetName) &&
                            typeinfo.IsClass && typeinfo.IsPublic &&
                            (additionalFilter?.Invoke(typeinfo) ?? true))
                        .ToList();

                    vals.AddRange(allRootTypes);
                    
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