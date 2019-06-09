using Lighthouse.Core.Logging;
using Lighthouse.Core.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.Events.Time
{
	public class TimeEventConsumer : BaseEventConsumer
	{
		public Dictionary<Schedule, DateTime?> Schedules { get; private set; }

		public Action<DateTime> EventAction { get; set;}

		public override IList<Type> Consumes { get; } = new[] { typeof(TimeEvent) };

		private IScheduleHistoryRepository ScheduleHistoryRepository { get; set; } 

		public TimeEventConsumer(IScheduleHistoryRepository scheduleHistoryRepository = null)
		{
			Schedules = new Dictionary<Schedule, DateTime?>();
			ScheduleHistoryRepository = scheduleHistoryRepository;
		}

		/// <summary>
		/// Handles <see cref="TimeEvent"/>s
		/// </summary>
		/// <param name="timeEvent"></param>
		public void HandleEvent(TimeEvent timeEvent)
		{
			Container.Log(LogLevel.Debug, LogType.EventReceived, this, timeEvent.ToString());

            // evaluate all of the schedules to see if one is a hit, if so, then run the action configured for this consumer
            if (Schedules.Any(s => s.Key.IsMatch(Schedules[s.Key], timeEvent.EventTime)))
            {
                Container.Do((container) => EventAction?.Invoke(timeEvent.EventTime));
                //Schedules.Where
            }
        }

		public void AddSchedule(Schedule schedule)
		{
			// no schedules have run yet, so null them out			
			Schedules.Add(schedule, ScheduleHistoryRepository?.GetLastRunDate(schedule));
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
