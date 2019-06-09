using Lighthouse.Core.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Scheduling
{
	/// <summary>
	/// Defines a pattern for when this schedule matches. This doesn't requiring knowing anything about previous runs. As it takes it's metadata to know when it should run.	
	/// </summary>
	public class Schedule
	{
		public string Name { get; private set; }
		public ScheduleFrequency Frequency { get; set; }
		public DateTime TimeToRun { get; set; }		
		public double FrequencyUnit { get; set; }
		
		public Schedule(ScheduleFrequency frequency,  double frequencyUnit = 1, string name = null)
		{
			Frequency = frequency;
			FrequencyUnit = frequencyUnit;
			
			// TODO: improve the quality of these name
			Name = name ?? $"Every {FrequencyUnit} {Frequency}";
		}

		public Schedule(DateTime timeToRun, string name = null)
		{
			TimeToRun = timeToRun;
			Frequency = ScheduleFrequency.Once;
			Name = name ?? TimeToRun.ToString("g");
		}

		public DateTime GetNextRunTime(DateTime? lastRun, DateTime now)
		{
			// never run again
			var nextRunTime = DateTime.MaxValue;

			switch (Frequency)
			{
				case ScheduleFrequency.Once:
					nextRunTime = TimeToRun;
					break;
				case ScheduleFrequency.Daily: // every _ days
					nextRunTime = lastRun?.AddDays(FrequencyUnit) ?? now; // if it's never run, now's the time
					break;
				case ScheduleFrequency.Hourly: // every _ hours
					nextRunTime = lastRun?.AddHours(FrequencyUnit) ?? now; // if it's never run, now's the time
					break;
				case ScheduleFrequency.Minutely: 
					nextRunTime = lastRun?.AddMinutes(FrequencyUnit) ?? now;
					break;
				case ScheduleFrequency.Secondly: // every _ seconds
					nextRunTime = lastRun?.AddSeconds(FrequencyUnit) ?? now;
					break;
			}

            return nextRunTime;
		}

		public bool IsMatch(DateTime? lastRun, DateTime now)
		{	
			// if the next time it should run is now or in the past, run right now
			// TODO: do some work to create a window (remove seconds? anything else?)
			return GetNextRunTime(lastRun, now) <= now;
		}
	}

	public enum ScheduleFrequency
	{
		Once,
		Daily,		
		Hourly,
		Minutely,
		Secondly			
	}
}
