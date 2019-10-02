using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core
{
    public interface IResourceProvider
	{
    }

    public class ResourceProviderConfig
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public string ConnectionString { get; set; }
    }

    public static class ResourceFactory
    {
        public static (bool wasSuccessful, string errorReason) TryCreate(ResourceProviderConfig config, out IResourceProvider resourceProvider)
        {
            resourceProvider = null;

            if (Enum.TryParse<ResourceProviderConfigType>(config.Type, out var configType))
            {
                switch (configType)
                {
                    case ResourceProviderConfigType.Database:
                        var (worked, errorReason) = DatabaseResourceFactory.TryParse(config, out var databaseResourceProvider);

                        if (worked)
                        {
                            resourceProvider = databaseResourceProvider;
                        }

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

    public enum ResourceProviderConfigType
    {
        Database,

    }

    public static class DatabaseResourceFactory
    {
        public static (bool wasSuccessful, string errorReason) TryParse(ResourceProviderConfig config, out DatabaseResourceProvider provider)
        {
            provider = null;


            return (true, null);
        }
    }

    public class DatabaseResourceProvider : IResourceProvider
    {

    }
}
