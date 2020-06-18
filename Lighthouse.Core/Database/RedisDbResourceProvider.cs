using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lighthouse.Core.Database
{
    public class RedisDbResourceProvider : LighthouseServiceBase, IKeyValueDatabaseProvider
    {
        public ResourceProviderType Type => ResourceProviderType.Database;
        public string ConnectionString { get; set; }

        public string Descriptor { get; private set; } = "redis";

        public Task<string> Query(string queryObject)
        {
            throw new NotImplementedException();
        }

        public void Register(ILighthouseServiceContainer peer, Dictionary<string, string> otherConfig = null)
        {
        }
    }
}
