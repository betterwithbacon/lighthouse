using Lighthouse.Core.Events.Queueing;
using Lighthouse.Core.Events.Time;
using Lighthouse.Core.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lighthouse.Core.Events
{
	//public class EventContext : IEventContext
	//{
	//	readonly ConcurrentBag<IEventProducer> Producers = new ConcurrentBag<IEventProducer>();
	//	readonly ConcurrentDictionary<Type, IList<IEventConsumer>>  Consumers = new ConcurrentDictionary<Type, IList<IEventConsumer>>();
	//	readonly ConcurrentBag<IEvent> AllReceivedEvents = new ConcurrentBag<IEvent>();		
	//	TimeEventProducer GlobalClock { get; set; } // raise an event every minute, like a clock (a not very good clock)
	//	public IWorkQueue<IEvent> EventQueue { get; }
	//	bool IsInited { get; set; }
	//	IWorkQueue<IEvent> WorkQueue { get; set; }
	//	public string Id { get; private set; }

	//	public ILighthouseServiceContainer LighthouseContainer { get; }

	//	public EventContext(ILighthouseServiceContainer container, IWorkQueue<IEvent> eventQueue = null, double defaultScheduleTimeIntervalInMilliseconds = 60 * 1000)
	//	{
	//		LighthouseContainer = container;
	//		EventQueue = eventQueue ?? new MemoryEventQueue();
	//		GlobalClock = new TimeEventProducer(defaultScheduleTimeIntervalInMilliseconds);
	//	}

	//	public void Initialize()
	//	{
	//		if (!IsInited)
	//		{
	//			Id = GenerateSessionIdentifier(this);

	//			// the default action is to write it to the internal session messages store			
	//			//AddLogAction((m) => SessionLogMessages.Add(m));

	//			// start the global clock, it'll emit events based on time.				
	//			RegisterProducer(GlobalClock);

	//			// configure a producer, that will periodically read from an event stream, and emit those events within the context.
	//			RegisterProducer(new QueueEventProducer(EventQueue, 1000));

	//			IsInited = true;
	//		}
	//	}

	//	#region Processing
	//	/// <summary>
	//	/// Will perform these actions in a context-attached, meaning they can potentially receive event,s if the worker threads have subscriber
	//	/// </summary>
	//	/// <param name="actions"></param>
	//	public void Do(IEnumerable<Action<ILighthouseServiceContainer>> actions)
	//	{
	//		// just run all of the tasks
	//		foreach (var action in actions)
	//			action(this);
	//	}
	//	#endregion

	//	#region Events
	//	Timer Timer;

	//	public event StatusUpdatedEventHandler StatusUpdated;

	//	void PollForEvents()
	//	{
	//		// kick off the timer
	//		// TODO: the creation of the handler should be somewhere else probably
	//		Timer = new Timer(
	//			(context) => {
	//				//var eventContext = context as EventContext;
	//				try
	//				{
	//					var ev = WorkQueue.Dequeue(1).FirstOrDefault();
	//					if (ev != null)
	//					{
	//						HandleEvent(ev);
	//					}
	//				}
	//				catch (Exception e)
	//				{
	//					LogError(e);
	//					throw;
	//				}
	//			}, this, 100, 1000
	//			);
	//	}

	//	public void RegisterProducer(IEventProducer eventProducer)
	//	{
	//		// add it
	//		Producers.Add(eventProducer);

	//		// and register this as the context with the producer
	//		//eventProducer.Init(this);

	//		Log(LogType.ProducerRegistered, source: eventProducer);

	//		AssertProducerIsReady(eventProducer);

	//		// after registered, go ahead and start the producer.
	//		eventProducer.Start();
	//	}

	//	public void AssertProducerIsReady(IEventProducer producer)
	//	{
	//		// if the containers aren't equal, this the producer's not ready.	
	//		if (producer.LighthouseContainer != this.LighthouseContainer)
	//			throw new ApplicationException($"Producer: {producer} is not ready.");
	//	}

	//	public void RegisterConsumer<TEvent>(IEventConsumer eventConsumer)
	//		where TEvent : IEvent
	//	{
	//		Consumers.GetOrAdd(typeof(TEvent), new List<IEventConsumer> { eventConsumer }); //?.Add(eventConsumer);

	//		eventConsumer.Init(this);

	//		Log(LogType.ConsumerRegistered, source: eventConsumer as ILighthouseLogSource);
	//	}

	//	public void EmitEvent(IEvent ev, ILighthouseLogSource logSource = null)
	//	{
	//		AssertIsInited();

	//		// log the event was raised within the context
	//		Log(LogType.EventSent, ev.ToString(), source: logSource);

	//		// ALL work should be enqueued for later execution. this means, that every event received, 
	//		// will be heard by both the local context, and potentially propagated to other contexts
	//		//EventQueue.Enqueue(ev);

	//		HandleEvent(ev);
	//	}

	//	private void AssertIsInited()
	//	{
	//		if (!IsInited)
	//			throw new InvalidOperationException("The context is not initialized.");
	//	}

	//	private void HandleEvent(IEvent ev)
	//	{
	//		if (ev == null)
	//			return;

	//		// handle tasks in a separate thread
	//		_ = Task.Run(() =>
	//		{
	//			AllReceivedEvents.Add(ev);

	//			if (Consumers.TryGetValue(ev.GetType(), out var consumers))
	//			{
	//				foreach (var consumer in consumers)
	//					consumer.HandleEvent(ev);
	//			}
	//		});
	//	}

	//	#endregion

	//	//#region Logging
	//	//public void AddLogAction(Action<LogMessage> messageAction)
	//	//{
	//	//	SessionLogActions.Add(messageAction);
	//	//}

	//	public void Log(LogType logType, string message = null, ILighthouseLogSource source = null)
	//	{
	//		//Log(new LogMessage
	//		//{
	//		//	LogType = logType,
	//		//	Time = DateTime.Now,
	//		//	Message = message,
	//		//	Source = source
	//		//});
	//	}

	//	public void LogError(Exception exception, string message = null, ILighthouseLogSource source = null)
	//	{
			
	//	}

	//	//public void Log(LogMessage message)
	//	//{
	//	//	foreach (var logAction in SessionLogActions)
	//	//	{
	//	//		_ = Task.Run(() => logAction(message));
	//	//	}
	//	//}
	//	//#endregion
}
