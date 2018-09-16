using System;
using System.Collections.Generic;
using System.Text;
using BusDriver.Core.Events.Time;

namespace Lighthouse.Core.Scheduling
{
	/// <summary>
	/// Defines a pattern for when this schedule matches. This doesn't requiring knowing anything about previous runs. As it takes it's metadata to know when it should run.	
	/// </summary>
	public class Schedule
	{
		public ScheduleFrequency Frequency { get; set; }
		public DateTime TimeToRun { get; set; }		
		public decimal FrequencyUnit { get; set; }
		
		public bool IsMatch(DateTime time)
		{
			switch(Frequency)
			{
				case ScheduleFrequency.Once:
					return TimeToRun.Date == time.Date && TimeToRun.Hour == time.Hour && TimeToRun.Minute == time.Minute;
				case ScheduleFrequency.OncePerDay: // the hour/minute per day to run
					return TimeToRun.Hour == time.Hour && TimeToRun.Minute == time.Minute;
				case ScheduleFrequency.OncePerHour: // the minute per hour to run
					return FrequencyUnit == time.Minute;
				case ScheduleFrequency.OnceEveryUnitsMinute: // the second per minute to run
					return FrequencyUnit == time.Second;
				default:
					return false; // what schedule frequency is this?					
			}
		}
	}

	public enum ScheduleFrequency
	{
		Once,
		OncePerDay,		
		OncePerHour,
		OnceEveryUnitsMinute		
	}
}
