using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lighthouse.Core.Database;

namespace Lighthouse.Core
{
    public static class DatabaseResourceFactory
    {
        private static IList<string> AvailableSubTypes;

        static DatabaseResourceFactory()
        {
            AvailableSubTypes = Enum
                    .GetValues(typeof(DatabaseResourceProviderConfigSubtype))
                    .OfType<DatabaseResourceProviderConfigSubtype>()
                    .Select(w => w.ToString())
                    .ToList();
        }

        public static (bool wasSuccessful, string errorReason) TryCreate(ResourceProviderConfig config, out IDatabaseResourceProvider<string> provider)
        {
            provider = null;

            if (string.IsNullOrEmpty(config.SubType))
                return (false, "no subtype provided");

            if (Enum.TryParse(typeof(DatabaseResourceProviderConfigSubtype), config.SubType.ToLower(), out var subtype))
            {
                switch (subtype)
                {
                    case DatabaseResourceProviderConfigSubtype.in_memory_key_value:
                        provider = new MsSqlDbResourceProvider
                        {
                            ConnectionString = config.ConnectionString
                        };
                        break;
                    case DatabaseResourceProviderConfigSubtype.sqlserver:
                        provider = new MsSqlDbResourceProvider
                        {
                            ConnectionString = config.ConnectionString
                        };
                        break;
                    case DatabaseResourceProviderConfigSubtype.redis:
                        provider = new RedisDbResourceProvider
                        {
                            ConnectionString = config.ConnectionString
                        };
                        break;
                    default:
                        return (false, $"subtype {config.SubType} is invalid.");
                }

                return (true, null);
            }

            

            return (false,errorReason: $"subtype {config.SubType} is invalid. All valid subtypes for {config.Type} are {string.Join(",",AvailableSubTypes)}");
        }
    }
}
