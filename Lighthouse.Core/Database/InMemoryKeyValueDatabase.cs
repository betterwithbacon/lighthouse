using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lighthouse.Core.Database
{
    [ExternalLighthouseService("in_mem_key_val_server")]
    public class InMemoryKeyValueDatabase : LighthouseServiceBase, ILighthouseServiceHasState
    {
        readonly ConcurrentDictionary<string, string> Data = new ConcurrentDictionary<string, string>();

        public override string Identifier { get; }

        public InMemoryKeyValueDatabase(string identifier)
        {
            Identifier = identifier;
        }

        public string Retrieve(string key)
        {
            return Data.TryGetValue(key, out var val) ? val : null;
        }

        public void Store(string key, string payload)
        {
            Data.AddOrUpdate(key, payload, (s, k) => payload);
        }

        public IEnumerable<string> GetState()
        {
            yield return $"{Data.Count} records";
        }
    }

    public class InMemoryKeyValQueryRequest
    {
        public string Key { get; set; }
    }

    public class InMemoryKeyValProvider : LighthouseServiceBase, IKeyValueDatabaseProvider
    {
        public string Descriptor => "in_mem_key_val";

        public string ConnectionString { get; set; }

        public ResourceProviderType Type => ResourceProviderType.Database;

        public async Task<string> Query(string queryObject)
        {
            // so this server is either in cluster or in a remote cluster, if it's remote then we need a remote client
            var connection = TryParse(this.ConnectionString);

            // talk to the network to create a client to talk to another node somewhere else
            // TODO: probably should find a network provider that makes sense for the type of request. But right now there's only one type.
            var network = Container.GetNetworkProvider();
            return await network.GetObjectAsync<InMemoryKeyValQueryRequest, string>(connection.Address, new InMemoryKeyValQueryRequest { Key = queryObject });
        }

        public void Register(ILighthouseServiceContainer peer, Dictionary<string, string> otherConfig = null)
        {

        }

        /// <summary>
        /// A connection string should be in the form of "address=127.0.0.1;name=in_mem_key_val_store"
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static InMemoryKeyValueDatabaseConnection TryParse(string connectionString)
        {
            var tokens = connectionString
                .Split(";")
                .ToList()
                .Select(val => val.Split("="))
                .ToDictionary(k => k[0], v => v[1]);
            var conn = new InMemoryKeyValueDatabaseConnection();

            // refeflect against the object with the 

            return conn;
        }
    }

    public sealed class InMemoryKeyValueDatabaseConnection
    {
        public Uri Address { get; set; }
        public string Name { get; set; }

        public static string ToString(string serverName, string identifier) => $"address={serverName};name={identifier}";
    }
}
