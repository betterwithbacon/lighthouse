using BusDriver.Core.Queueing;
using System;
using System.Linq;
using System.Threading;

namespace Lighthouse.Core.Events
{
	public class QueueEventProducer : BaseEventProducer
	{
		IWorkQueue<IEvent> WorkQueue { get; set; }		
		int DelayInMilliseconds { get; }
		Timer Timer;

		public QueueEventProducer(IWorkQueue<IEvent> workQueue, int delayInMilliseconds = 1 * 1000)
		{
			WorkQueue = workQueue ?? throw new ApplicationException("No work queue was provided");
			DelayInMilliseconds = delayInMilliseconds;
		}

		public override void Start()
		{
			// kick off the timer
			// TODO: the creation of the handler should be somewhere else probably
			Timer = new Timer(
				(context ) => {
					var eventContext = context as IEventContext;
					try
					{
						var ev = WorkQueue.Dequeue(1).FirstOrDefault();
						if (ev != null)
						{
							eventContext.RaiseEvent(ev, this);
						}
					}catch(Exception e)
					{
						eventContext.LogError(e, source: this);
						throw;
					}
				}, Context, 100, 1000
			);
		}
	}
}
