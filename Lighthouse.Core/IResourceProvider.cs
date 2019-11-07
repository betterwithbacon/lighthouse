using Lighthouse.Core.Database;
using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core
{
    public interface IResourceProvider
    {
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

    public class ResourceProviderConfig
    {
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

    public enum ResourceProviderConfigType
    {
        Database,
    }

    public static class DatabaseResourceProviderConfigSubtype
    {
        public const string SqlServer = "sqlserver";
        public const string Redis = "redis";
        
    }

    
    public static class DatabaseResourceFactory
    {
        public static (bool wasSuccessful, string errorReason) TryCreate(ResourceProviderConfig config, out IDatabaseResourceProvider<string> provider)
        {
            provider = null;

            if (string.IsNullOrEmpty(config.SubType))
                return (false, "no subtype provided");

            switch(config.SubType.ToLower())
            {
                case DatabaseResourceProviderConfigSubtype.SqlServer:
                    provider = new MsSqlDbResourceProvider();
                    break;
                case DatabaseResourceProviderConfigSubtype.Redis:
                    provider = new RedisDbResourceProvider();
                    break;
                default:
                    return (false, $"subtype {config.SubType} is invalid.");
            }

            return (true, null);
        }
    }

    public class DatabaseResourceProvider : IResourceProvider
    {
    }
}
