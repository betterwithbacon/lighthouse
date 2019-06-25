using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Lighthouse.Server
{
    public static class LighthouseFetcher
    {
        public static ConcurrentBag<Type> AllFoundTypes = new ConcurrentBag<Type>();

        public static Type Fetch(string serviceName)
        {
            // load local assemblies            	
            foreach (var service in
                        DllTypeLoader.Load<ILighthouseService>(                            
                                (t) =>
                                {
                                    var attrs = t.CustomAttributes.Where(att => att.AttributeType.Name == typeof(ExternalLighthouseServiceAttribute).Name);
                                    if (attrs != null && attrs.Count() > 0)
                                    {
                                        if ((attrs.First().ConstructorArguments[0].Value as string).Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }
                    ))
            {
                return service;
            }

            return null;
        }
    }
}
