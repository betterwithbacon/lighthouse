﻿
using BusDriver.Core.Logging;
using BusDriver.Core.Scheduling;
using Lighthouse.Core.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lighthouse.Core.Events.Time
{
	public class TimeEventConsumer : IEventConsumer
	{
		public IList<Type> Consumes => new[] { typeof(TimeEvent) };

		public List<Schedule> Schedules { get; private set; }

		public Action<DateTime> EventAction { get; set;}
		
		IEventContext Context { get; set; }

		public string Identifier { get; private set; }

		public TimeEventConsumer()
		{
			Schedules = new List<Schedule>();
		}

		public void HandleEvent(IEvent ev)
		{
			this.ThrowIfInvalidEvent(ev);

			var timeEvent = ev as TimeEvent;

			//TODO: Context.Log(LogType.EventReceived, timeEvent.ToString(), source: this);

			// evaluate all of the schedules to see if one is a hit, if so, then run the action configured for this consumer
			if (Schedules.Any(s => s.IsMatch(timeEvent.Time)))
			{
				EventAction(ev.Time);
			}
		}

		public void Init(IEventContext context)
		{
			Context = context;
			Identifier = EventContext.GenerateSessionIdentifier(this);
			//TODO: Context.Log(LogType.ConsumerStartup, source: this);
		}
	}
}
