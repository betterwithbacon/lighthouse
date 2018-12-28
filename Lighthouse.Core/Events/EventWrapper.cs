using System;
using System.Linq;

namespace Lighthouse.Core.Events
{
	public sealed class EventWrapper<T>
		where T : IEvent
	{
		public ILighthouseServiceContainer Container { get; private set; }
		public DateTime EventTime { get; private set; }

		public T Event { get; }

		public EventWrapper(ILighthouseServiceContainer container, DateTime eventTime, T ev)
		{
			Container = container;
			EventTime = eventTime;
			Event = ev;
		}
	}

	public static class IEventExtensions
	{
		public static void ThrowIfInvalidEvent(this IEventConsumer consumer, EventWrapper<IEvent> ev)
		{
			if (!consumer.Consumes.Contains(ev?.GetType()))
				throw new InvalidEventException(ev.GetType(), consumer.Consumes.ToArray());
		}

		public static EventWrapper<T> Wrap<T>(this T @event, ILighthouseServiceContainer container, DateTime eventTime)
			where T : IEvent
		{
			return new EventWrapper<T>(container, eventTime, @event);
		}
	}
}