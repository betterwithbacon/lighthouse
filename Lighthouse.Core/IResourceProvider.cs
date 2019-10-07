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
                        var (worked, errorReason) = DatabaseResourceFactory.TryCreate(config, out var databaseResourceProvider);

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

    public enum DatabaseResourceProviderConfigSubtype
    {
        SqlServer,
        Redis
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
                case "sqlserver":
                    provider = new MsSqlDbResourceProvider();
                    break;
                case "redis":
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
