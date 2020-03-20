﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.Database
{
    public interface IDatabaseResourceProvider : IResourceProvider
    {
        string Descriptor { get; }
    }

    public interface IDatabaseResourceProvider<TQueryObject> : IDatabaseResourceProvider
    {
        Task<IEnumerable<TResult>> Query<TResult>(TQueryObject queryObject);
    }

    public class RedisDbResourceProvider : LighthouseServiceBase, IDatabaseResourceProvider<string>
    {
        public ResourceProviderType Type => ResourceProviderType.Database;
        public string ConnectionString { get; set; }

        public string Descriptor { get; private set; } = "redis";

        public Task<IEnumerable<TResult>> Query<TResult>(string queryObject)
        {
            throw new NotImplementedException();
        }

        public void Register(ILighthousePeer peer, Dictionary<string, string> otherConfig = null)
        {
            throw new NotImplementedException();
        }
    }

    public class MsSqlDbResourceProvider : LighthouseServiceBase, IDatabaseResourceProvider<string>
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

        public async Task<IEnumerable<TResult>> Query<TResult>(string queryObject)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                return await connection.QueryAsync<TResult>(queryObject);
            }
        }

        public void Register(ILighthousePeer peer, Dictionary<string, string> otherConfig = null)
        {
            // do nothing
        }
    }
}
