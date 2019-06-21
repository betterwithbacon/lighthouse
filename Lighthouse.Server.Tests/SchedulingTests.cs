using System;
using Xunit;
using FluentAssertions;
using Xunit.Abstractions;
using System.Threading;
using Lighthouse.Core.Scheduling;
using Lighthouse.Core.Events.Time;
using Lighthouse.Core.Tests.Events;
using Lighthouse.Core.Utils;

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
			GivenAContainer(defaultScheduleTimeIntervalInMilliseconds: 25D);

			var timeSecond = DateTime.Now.Second;

			// create a schedule that will only fire the Action when the time matches the event time
			Container.AddScheduledAction(
				new Schedule(ScheduleFrequency.Secondly,5),
				(hitTime) => { Output.WriteLine($"EVENT HIT: {hitTime}"); reached = true; }
			);

			// run and ensure the listeners are all responding
			Container.Start();

			Thread.Sleep(50); // just wait a bit for the events to be handled
			Output.WriteLine($"[{DateTime.Now.ToLighthouseLogString()}] End TEST sleep phase.");

			// a few time e vents may be release
			//Container.AssertEventExists<TimeEvent>(atLeast: 1);
			reached.Should().BeTrue();
		}
	}
}
