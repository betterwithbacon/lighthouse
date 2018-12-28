
using Lighthouse.Core.Utils;
using System;

namespace Lighthouse.Core.Events.Time
{
	public class TimeEvent : BaseEvent
	{
		public TimeEvent(ILighthouseServiceContainer container, DateTime time)
			: base(container,time)
		{
			LighthouseContainer = container;
			EventTime = time;			
		}

		public override string ToString()
		{
			return $"Time: {EventTime.ToLighthouseLogString()}";
		}
	}
}