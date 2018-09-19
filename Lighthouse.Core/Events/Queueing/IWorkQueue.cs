using System.Collections.Generic;

namespace Lighthouse.Core.Events.Queueing
{
    public interface IWorkQueue<out T>
    {
		IEnumerable<T> Dequeue(int count);
		void Enqueue(IEvent ev);
    }
}
