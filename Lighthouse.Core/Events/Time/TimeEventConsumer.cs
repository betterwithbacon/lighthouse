using Lighthouse.Core.Logging;
using Lighthouse.Core.Scheduling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lighthouse.Core.Events.Time
{
	public class TimeEventConsumer : BaseEventConsumer
	{
		public Schedule Schedule { get; private set; }
        private DateTime? lastRunTime; // { get; private set; }
        private static readonly object locker = new object();

        public Action<DateTime> EventAction { get; set;}

		public override IList<Type> Consumes { get; } = new[] { typeof(TimeEvent) };

		private IScheduleHistoryRepository ScheduleHistoryRepository { get; set; } 

		public TimeEventConsumer(IScheduleHistoryRepository scheduleHistoryRepository = null)
		{
			ScheduleHistoryRepository = scheduleHistoryRepository;
		}

		/// <summary>
		/// Handles <see cref="TimeEvent"/>s
		/// </summary>
		/// <param name="timeEvent"></param>
		public void HandleEvent(TimeEvent timeEvent)
		{
            if (Schedule != null && EventAction != null)
            {
                var nextRunTime = Schedule.GetNextRunTime(lastRunTime, Container.GetNow());
                var now = Container.GetNow();
                Container.Log(LogLevel.Debug, LogType.EventReceived, this, $"now: {now}, event time: {timeEvent.ToString()}: nextRunTime: {nextRunTime}");

                if (nextRunTime <= now)
                {
                    Container.Do((container) => EventAction?.Invoke(timeEvent.EventTime));

                    lock (locker)
                    {
                        lastRunTime = now;
                    }
                }
            }
        }

		public void SetSchedule(Schedule schedule)
		{
            // no schedules have run yet, so null them out			
            Schedule = schedule;

            // also load new schedule history?
            //var lastRunTime = ScheduleHistoryRepository.GetLastRunDate(Schedule);
        }

		protected override void OnInit()
		{
			if (ScheduleHistoryRepository == null)
			{
				//ScheduleHistoryRepository = Container.FindServices<IScheduleHistoryRepository>().FirstOrDefault();
			}
		}
	}
}
