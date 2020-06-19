using System;

namespace Lighthouse.Core.Database
{
    public struct DateRange
    {
        public DateTime? StartDate { get; }
        public DateTime? EndDate { get; }

        public DateRange(DateTime? startDate = null, DateTime? endDate = null)
        {
            StartDate = startDate;
            EndDate = endDate;
        }
    }

    public class Record<T>
    {
        private T value;

        public Record(T value)
        {
            this.value = value;
        }

        public DateTime Time { get; }
        public T Value { get; }
    }
}