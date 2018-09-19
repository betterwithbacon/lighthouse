using System;
using Xunit;
using FluentAssertions;
using Xunit.Abstractions;
using System.Threading;
using Lighthouse.Core.Scheduling;
using Lighthouse.Core.Events.Time;
using Lighthouse.Core.Tests.Events;

namespace Lighthouse.Server.Tests
{
	public class SchedulingTests : LighthouseServerTests
	{
		public SchedulingTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		[Trait("Category", "Unit")]
		public void AddSchedule_ShouldAddCauseEventToBeTriggeredFromSchedule()
		{
			bool reached = false;

			// create the orchestrator
			var container = GivenAContainer(defaultScheduleTimeIntervalInMilliseconds: 25D);

			var timeSecond = DateTime.Now.Second;

			// create a schedule that will only fire the Action when the time matches the event time
			container.AddScheduledAction(new Schedule { Frequency = ScheduleFrequency.OnceEveryUnitsMinute, FrequencyUnit = timeSecond },
				(hitTime) => { Output.WriteLine($"EVENT HIT: {hitTime}"); reached = true; });

			// run and ensure the listeners are all responding
			container.Start();

			Thread.Sleep(50); // just wait a bit for the events to be handled

			// a few time e vents may be release
			container.AssertEventExists<TimeEvent>(atLeast: 1);
			reached.Should().BeTrue();
		}
	}
}
