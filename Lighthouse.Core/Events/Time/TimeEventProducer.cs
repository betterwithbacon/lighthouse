
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
			Container.EmitEvent(new TimeEvent(Container, e.SignalTime), this);
		}

		public override void Start()
		{
			Timer.Start();
		}
	}
}
