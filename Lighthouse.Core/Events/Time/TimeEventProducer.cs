
using BusDriver.Core.Logging;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Lighthouse.Core.Events.Time
{
    public class TimeEventProducer : BaseEventProducer
	{
		readonly Timer Timer = new Timer();

		public TimeEventProducer(double intervalInMilliseconds)
		{
			Timer.Interval = intervalInMilliseconds;
			Timer.Elapsed += Timer_Elapsed;
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Context.RaiseEvent(new TimeEvent(Context, e.SignalTime), this);
		}

		public override void Start()
		{
			Timer.Start();
		}
	}
}
