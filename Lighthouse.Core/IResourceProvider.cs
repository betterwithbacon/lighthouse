using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core
{
    public interface IResourceProvider
    {
        ResourceProviderType Type { get; }
        void Register(ILighthousePeer peer, Dictionary<string, string> otherConfig = null);
    }

    public abstract class ConfigBase
    {
        public string Name { get; set; }
    }

    public class LighthouseRunConfig : ConfigBase
    {
        public Dictionary<string, ResourceProviderConfig> Resources { get; set; }
        public Dictionary<string, ApplicationConfig> Applications { get; set; }
    }

    public class ApplicationConfig : ConfigBase
    {
        public string TypeName { get; set; }
        public string Configuration { get; set; }
    }

    public static class ResourceFactory
    {
        public static (bool wasSuccessful, string errorReason) TryCreate(ResourceProviderConfig config, out IResourceProvider resourceProvider)
        {
            resourceProvider = null;

            if (Enum.TryParse<ResourceProviderType>(config.Type, out var configType))
            {
                switch (configType)
                {
                    case ResourceProviderType.Database:
                        var (worked, errorReason) = DatabaseResourceFactory.TryCreate(config, out var databaseResourceProvider);

                        if (!worked)
                        {
                            return (worked, errorReason);
                        }
                        resourceProvider = databaseResourceProvider;

                        return (worked, errorReason);
                }
            }
            else
            {

                return (false, "Resource type could not be parsed.");
            }

            return (false, null);
        }
    }
}
