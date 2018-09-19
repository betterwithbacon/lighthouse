using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lighthouse.Core.Events.Queueing
{
	// a simple in-memory queue for events.
	public class MemoryEventQueue : IWorkQueue<IEvent>
	{
		private ConcurrentQueue<IEvent> Queue { get; }

		public MemoryEventQueue(ConcurrentQueue<IEvent> eventQueue = null)
		{
			Queue = eventQueue ?? new ConcurrentQueue<IEvent>();
		}
		
		public IEnumerable<IEvent> Dequeue(int count)
		{
			for (int i = 0; i < count && Queue.Count > 0; i++)
				if (Queue.TryDequeue(out var result))
					yield return result;
				else
					yield return null;
		}

		public void Enqueue(IEvent ev)
		{
			Queue.Enqueue(ev);
		}
	}
}
