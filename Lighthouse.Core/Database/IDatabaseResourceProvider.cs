using Dapper;
using System;
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
}
