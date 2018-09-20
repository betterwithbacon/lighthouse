using Lighthouse.Core.Logging;
using Lighthouse.Core.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lighthouse.Core.Events.Time
{
	public class TimeEventConsumer : BaseEventConsumer
	{
		public List<Schedule> Schedules { get; private set; }

		public Action<DateTime> EventAction { get; set;}

		public override IList<Type> Consumes { get; } = new[] { typeof(TimeEvent) };

		public TimeEventConsumer()
		{
			Schedules = new List<Schedule>();
		}

		/// <summary>
		/// Handles <see cref="TimeEvent"/>s
		/// </summary>
		/// <param name="timeEvent"></param>
		public void HandleEvent(TimeEvent timeEvent)
		{
			LighthouseContainer.Log(LogLevel.Debug, LogType.EventReceived, this, timeEvent.ToString());

			// evaluate all of the schedules to see if one is a hit, if so, then run the action configured for this consumer
			if (Schedules.Any(s => s.IsMatch(timeEvent.Time)))
			{
				LighthouseContainer.Do((o) => EventAction(timeEvent.Time));
			}
		}

		//public override void HandleEvent(IEvent ev)
		//{
		//	this.ThrowIfInvalidEvent(ev);

		//	var timeEvent = ev as TimeEvent;

		//	LighthouseContainer.Log(LogLevel.Debug, LogType.EventReceived, this, timeEvent.ToString());

		//	// evaluate all of the schedules to see if one is a hit, if so, then run the action configured for this consumer
		//	if (Schedules.Any(s => s.IsMatch(timeEvent.Time)))
		//	{
		//		EventAction(ev.Time);
		//	}
		//}
	}
}
