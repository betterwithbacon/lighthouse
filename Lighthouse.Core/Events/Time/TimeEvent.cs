
using Lighthouse.Core.Utils;
using System;

namespace Lighthouse.Core.Events.Time
{
	public class TimeEvent : IEvent
	{
		public DateTime Time { get;  set; }

		public ILighthouseServiceContainer LighthouseContainer { get;  private set; }

		public TimeEvent(ILighthouseServiceContainer container, DateTime time)
		{
			LighthouseContainer = container;
			Time = time;			
		}

		public override string ToString()
		{
			return $"Time: {Time.ToLighthouseLogString()}";
		}
	}
}