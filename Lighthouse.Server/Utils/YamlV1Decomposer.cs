using System;
using System.Collections.Generic;
using Lighthouse.Core;

namespace Lighthouse.Server.Utils
{
    public static class YamlV1Decomposer
    {
        public static (IEnumerable<ResourceProviderConfig> Resources, IEnumerable<Type> Types) Deserialize(string fileContents)
        {
            var resources = new List<ResourceProviderConfig>();
            var types = new List<Type>();

            
            return (resources, types);
        }
    }
}
