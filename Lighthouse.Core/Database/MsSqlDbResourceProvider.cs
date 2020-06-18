using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lighthouse.Core.Database
{
    public class MsSqlDbResourceProvider : LighthouseServiceBase, ITabularDatabaseProvider
    {
        public ResourceProviderType Type => ResourceProviderType.Database;

        public string ConnectionString { get; set; }

        public Action<MsSqlDbResourceProvider> InitializeConnectionFunc { get; set; }

        public string Descriptor { get; private set; } = "sql_server";

        protected override void OnInit(object context = null)
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

        public void Register(ILighthouseServiceContainer peer, Dictionary<string, string> otherConfig = null)
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
