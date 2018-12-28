using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Events
{
	public interface IEvent
	{
		ILighthouseServiceContainer LighthouseContainer { get; }
		DateTime EventTime { get; }
	}

	public abstract class BaseEvent : IEvent
	{
		public ILighthouseServiceContainer LighthouseContainer { get; protected set; }

		public DateTime EventTime { get; protected set; }
		public ILighthouseServiceContainer Container { get; }

		public BaseEvent(ILighthouseServiceContainer container, DateTime? eventTime = null)
		{
			Container = container;
			EventTime = eventTime ?? container.GetNow();
		}
	}
}
