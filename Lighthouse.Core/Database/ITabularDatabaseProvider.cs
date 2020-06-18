using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lighthouse.Core.Database
{
    public interface ITabularDatabaseProvider : IDatabaseResourceProvider<string, IEnumerable<object>>
    {
        Task<TResult> Query<TResult>(string queryObject);
    }
}
