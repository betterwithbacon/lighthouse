using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lighthouse.Core.Database
{
    public interface ITimeSeriesDatabaseProvider<T> : IDatabaseResourceProvider
    {
        Task<IEnumerable<Record<T>>> Query(DateRange range);

        void Insert(DateTime time, object value);
    }

    /// <summary>
    /// The databas is it's own provider
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TimeSeriesDatabase<T> : ITimeSeriesDatabaseProvider<T>
    {
        public string Descriptor => $"Time series database ({typeof(T)})";

        public ResourceProviderType Type => ResourceProviderType.Database;

        private ConcurrentDictionary<DateTime, T> Records { get; set; }

        public void Insert(DateTime time, object value)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Record<T>>> Query(DateRange range)
        {
            return await Task.FromResult(
                Records
                    .Where(r => r.Key >= (range.StartDate ?? DateTime.MinValue) && r.Key <= (range.EndDate ?? DateTime.MaxValue))
                    .Select(kvp => new Record<T>(kvp.Value))
                    .ToList());
        }

        public void Register(ILighthouseServiceContainer peer, Dictionary<string, string> otherConfig = null)
        {
        }
    }

}
