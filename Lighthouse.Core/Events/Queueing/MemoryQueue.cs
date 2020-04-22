using System.Collections.Concurrent;
using System.Collections.Generic;
using NSubstitute.Core;

namespace Lighthouse.Core.Events.Queueing
{
	// a simple in-memory queue for events.
	public class MemoryEventQueue : IWorkQueue
	{
		private ConcurrentQueue<object> Queue { get; }

		public MemoryEventQueue(ConcurrentQueue<object> eventQueue = null)
		{
			Queue = eventQueue ?? new ConcurrentQueue<object>();
		}
		
		public IEnumerable<T> Dequeue<T>(int count)
		{
			for (int i = 0; i < count && Queue.Count > 0; i++)
				if (Queue.TryDequeue(out var result))
					yield return (T)result;
				else
					yield return default;
		}

		public void Enqueue(object ev)
		{
			Queue.Enqueue(ev);
		}
	}
}
