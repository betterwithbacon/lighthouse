using Dapper;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.Database
{
    public interface IDatabaseResourceProvider : IResourceProvider
    {
        string Descriptor { get; }
    }

    public interface IDatabaseResourceProvider<in TQueryObject, TResult> : IDatabaseResourceProvider
    {
        Task<TResult> Query(TQueryObject queryObject);
    }

    public interface IKeyValueDatabaseProvider : IDatabaseResourceProvider<string,string>
    {
    }

    public interface ITabularDatabaseProvider : IDatabaseResourceProvider<string, IEnumerable<object>>
    {
        Task<TResult> Query<TResult>(string queryObject);
    }

    [ExternalLighthouseService("in_mem_key_val_server")]
    public class InMemoryKeyValueDatabase : LighthouseServiceBase, ILighthouseServiceHasState
    {
        readonly ConcurrentDictionary<string, string> Data = new ConcurrentDictionary<string, string>();

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
            var network = Container.GetNetworkProviders().First();
            return await network.GetObjectAsync<InMemoryKeyValQueryRequest, string>(connection.Address, new InMemoryKeyValQueryRequest { Key = queryObject });
        }

        public void Register(ILighthousePeer peer, Dictionary<string, string> otherConfig = null)
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
    }

    public class RedisDbResourceProvider : LighthouseServiceBase, IKeyValueDatabaseProvider
    {
        public ResourceProviderType Type => ResourceProviderType.Database;
        public string ConnectionString { get; set; }

        public string Descriptor { get; private set; } = "redis";

        public Task<string> Query(string queryObject)
        {
            throw new NotImplementedException();
        }

        public void Register(ILighthousePeer peer, Dictionary<string, string> otherConfig = null)
        {            
        }
    }

    public class MsSqlDbResourceProvider : LighthouseServiceBase, ITabularDatabaseProvider
    {
        public ResourceProviderType Type => ResourceProviderType.Database;

        public string ConnectionString { get; set; }

        public Action<MsSqlDbResourceProvider> InitializeConnectionFunc { get; set; }

        public string Descriptor { get; private set; } = "sql_server";

        protected override void OnInit()
        {
            InitializeConnectionFunc?.Invoke(this);
            
            // initialize this db provider
            if (ConnectionString == null)
            {
                throw new ApplicationException("No connection string is present.");
            }
        }

        //public async Task<IEnumerable<TResult>> Query<TResult>(string queryObject)
        //{
        //    using (var connection = new SqlConnection(ConnectionString))
        //    {
        //        await connection.OpenAsync();

        //        return await connection.QueryAsync<TResult>(queryObject);
        //    }
        //}

        public void Register(ILighthousePeer peer, Dictionary<string, string> otherConfig = null)
        {
            // do nothing
        }

        public Task<TResult> Query<TResult>(string queryObject)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<object>> Query(string queryObject)
        {
            throw new NotImplementedException();
        }
    }
}
