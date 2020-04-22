using System.Collections.Generic;

namespace Lighthouse.Core.Events.Queueing
{
    public interface IWorkQueue
    {
		IEnumerable<T> Dequeue<T>(int count);
		void Enqueue(object work);
    }
}
