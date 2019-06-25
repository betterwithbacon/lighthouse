using System;
using System.Collections.Generic;
using System.Linq;

namespace Lighthouse.Server
{
    public static class DllTypeLoader
    {
        public static IEnumerable<Type> Load<T>(Func<Type, bool> additionalFilter = null)
            where T : class
        {
            var targetName = typeof(T).FullName;
            
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.ExportedTypes)
                .Where(typeinfo =>
                    typeinfo.GetInterfaces().Select(t => t.FullName).Contains(targetName) &&
                    typeinfo.IsClass && typeinfo.IsPublic &&
                    (additionalFilter?.Invoke(typeinfo) ?? true))
                .ToList();
        }
    }
}
