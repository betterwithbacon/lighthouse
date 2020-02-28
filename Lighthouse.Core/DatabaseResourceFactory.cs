using System;
using System.Collections;
using Lighthouse.Core.Database;

namespace Lighthouse.Core
{
    public static class DatabaseResourceFactory
    {
        public static (bool wasSuccessful, string errorReason) TryCreate(ResourceProviderConfig config, out IDatabaseResourceProvider<string> provider)
        {
            provider = null;

            if (string.IsNullOrEmpty(config.SubType))
                return (false, "no subtype provided");

            if (Enum.TryParse(typeof(DatabaseResourceProviderConfigSubtype), config.SubType.ToLower(), out var subtype))
            {
                switch (subtype)
                {
                    case DatabaseResourceProviderConfigSubtype.sqlserver:
                        provider = new MsSqlDbResourceProvider();
                        break;
                    case DatabaseResourceProviderConfigSubtype.redis:
                        provider = new RedisDbResourceProvider();
                        break;
                    default:
                        return (false, $"subtype {config.SubType} is invalid.");
                }

                return (true, null);
            }
            return (false, $"subtype {config.SubType} is invalid. All valid subtypes for {config.Type} are {string.Join(",", (IEnumerable)Enum.GetValues(typeof(DatabaseResourceProviderConfigSubtype)))}");
        }
    }
}
