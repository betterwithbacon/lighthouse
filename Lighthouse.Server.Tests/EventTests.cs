using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using FluentAssertions;
using Xunit.Abstractions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Lighthouse.Server;
using Lighthouse.Core.Events.Logging;
using Lighthouse.Core.Scheduling;
using Lighthouse.Core.Events.Time;
using Lighthouse.Core.Events.Queueing;
using Lighthouse.Core.Tests.Events;

namespace Lighthouse.Server.Tests
{
	public class EventTests : LighthouseServerTests
	{
		public EventTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		[Trait("Category", "Unit")]
		public void QueueEventProducerShouldRetrieveEventAndPutIntocontainer()
		{
			var memQueue = new MemoryEventQueue();
			var container = GivenAContainer(memQueue);

			container.Start();

			var testEvent = new TestEvent(container);

			// raise the event
			//container.EmitEvent(testEvent, null);
			memQueue.Enqueue(testEvent);

			// give the system enough time to react to the event showing up
			Thread.Sleep(200);

			// look for non-time events
			container.AssertEventExists<TestEvent>();
		}

		[Fact]
		[Trait("Category", "Unit")]
		public void TimeEventShouldTriggerLogWriteEventWhichShouldThenWriteToLog()
		{
			// create the orchestrator
			var container = GivenAContainer();
			var time = DateTime.Parse("01/01/2018 10:00AM");

			// create a consumer that when it receives a time event, that matches it's schedule, it will trigger a log write event
			var timeEventConsumer = new TimeEventConsumer();
			timeEventConsumer.EventAction = (triggerTime) => container.EmitEvent(
				new LogEvent(timeEventConsumer)
				{
					Message = $"Log of: TimeEvent hit at: {triggerTime.Ticks}",
					Time = container.GetTime()
				}, timeEventConsumer
			);

			// create a schedule that will only fire the Action when the time matches the event time
			timeEventConsumer.AddSchedule(new Schedule { Frequency = ScheduleFrequency.Daily, TimeToRun = time });

			// create a consumer that will see the evenets and write to the log
			var logWriteConsumer = new LogEventConsumer();

			container.RegisterEventConsumer<TimeEvent>(timeEventConsumer);
			container.RegisterEventConsumer<LogEvent>(logWriteConsumer);

			// run and ensure the listeners are all responding
			container.Start();

			container.EmitEvent(new TimeEvent(container, time), null);
			Thread.Sleep(50); // just wait a bit for the events to be handled
			container.AssertEventExists<TimeEvent>();
			container.AssertEventExists<LogEvent>();

			Output.WriteLine(Environment.NewLine + "Warehouse Contents");
			logWriteConsumer.AllLogRecords.ForEach(Output.WriteLine);
			logWriteConsumer.AllLogRecords.Where(l => l.Contains(time.Ticks.ToString())).Count().Should().Be(1);
		}

		[Fact]
		[Trait("Category", "Unit")]
		public async Task TimeEventShouldTriggerOneAndOnlyOneSchedule()
		{
			// create the orchestrator
			var container = GivenAContainer();

			var time = DateTime.Parse("01/01/2018 10:00AM");

			// create a consumer that when it receives a time event, that matches it's schedule, it will trigger a log write event
			var timeEventConsumer = new TimeEventConsumer();
			timeEventConsumer.EventAction = (triggerTime) => container.EmitEvent(
				new LogEvent(timeEventConsumer)
				{
					Message = $"Log of: TimeEvent hit at: {triggerTime.Ticks}",
					Time = container.GetTime()
				}, timeEventConsumer
			);

			// create a schedule that will only fire the Action when the time matches the event time
			timeEventConsumer.AddSchedule(new Schedule { Frequency = ScheduleFrequency.Daily, TimeToRun = time });

			// create a consumer that will see the evenets and write to the log
			var logWriteConsumer = new LogEventConsumer();

			container.RegisterEventConsumer<TimeEvent>(timeEventConsumer);
			container.RegisterEventConsumer<LogEvent>(logWriteConsumer);

			// run and ensure the listeners are all responding
			container.Start();

			container.EmitEvent(new TimeEvent(container, time), null);
			container.EmitEvent(new TimeEvent(container, time.AddDays(-10)), null);
			container.EmitEvent(new TimeEvent(container, time.AddDays(10)), null);			
			await container.Stop();

			Thread.Sleep(500); // just wait a bit for the events to be handled
			

			container.AssertEventExists<TimeEvent>(3);
			//container.AssertEventExists<LogEvent>(atLeast: 3);

			Output.WriteLine(Environment.NewLine + "Warehouse Contents:");
			logWriteConsumer.AllLogRecords.ForEach(Output.WriteLine);
			logWriteConsumer.AllLogRecords.Count().Should().Be(4);
		}
	}
}
