
using System;
using System.Timers;

namespace Lighthouse.Core.Events.Time
{
    public class TimeEventProducer : BaseEventProducer
	{
		readonly Timer Timer = new Timer();
		bool isRunning = false;

		public TimeEventProducer(double intervalInMilliseconds)
		{
			Timer.Interval = intervalInMilliseconds;
			Timer.Elapsed += Timer_Elapsed;
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Container.EmitEvent(new TimeEvent(Container, e.SignalTime), this);
		}

		public override void Start()
		{
			Timer.Start();
			isRunning = true;
		}

		public void UpdateFrequency(double fireEvery)
		{
			// preserve the existing run state
			if(isRunning)
				Timer.Stop();

			Timer.Interval = fireEvery;

			if(isRunning)
				Timer.Start();
		}
	}
}
