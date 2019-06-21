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
			GivenAContainer(memQueue);

			Container.Start();

			var testEvent = new TestEvent(Container);

			// raise the event
			//Container.EmitEvent(testEvent, null);
			memQueue.Enqueue(testEvent);

			// give the system enough time to react to the event showing up
			Thread.Sleep(200);

			// look for non-time events
			Container.AssertEventExists<TestEvent>();
		}

		[Fact]
		[Trait("Category", "Unit")]
		public async Task TimeEventShouldTriggerLogWriteEventWhichShouldThenWriteToLog()
		{
			// create the orchestrator			
			var time = DateTime.Parse("01/01/2018 10:00AM");

			// create a consumer that when it receives a time event, that matches it's schedule, it will trigger a log write event
			var timeEventConsumer = new TimeEventConsumer();
			timeEventConsumer.EventAction = (triggerTime) => Container.EmitEvent(
				new LogEvent(Container, timeEventConsumer)
				{
					Message = $"Log of: TimeEvent hit at: {triggerTime.Ticks}"
				}, timeEventConsumer
			);

			// create a schedule that will only fire the Action when the time matches the event time
			timeEventConsumer.SetSchedule(new Schedule(time));

			// create a consumer that will see the evenets and write to the log
			var logWriteConsumer = new LogEventConsumer();

			Container.RegisterEventConsumer<TimeEvent>(timeEventConsumer);
			Container.RegisterEventConsumer<LogEvent>(logWriteConsumer);

			// run and ensure the listeners are all responding
			Container.Start();

			await Container.EmitEvent(new TimeEvent(Container, time), null);
			Thread.Sleep(50); // just wait a bit for the events to be handled
			Container.AssertEventExists<TimeEvent>();
			Container.AssertEventExists<LogEvent>();

			Output.WriteLine(Environment.NewLine + "Warehouse Contents");
			logWriteConsumer.AllLogRecords.ForEach(Output.WriteLine);
			logWriteConsumer.AllLogRecords.Where(l => l.Contains(time.Ticks.ToString())).Count().Should().Be(1);
		}

		[Fact]
		[Trait("Category", "Unit")]
		public async Task TimeEventShouldTriggerOneAndOnlyOneSchedule()
		{
			// create the orchestrator			
			var time = DateTime.Parse("01/01/2018 10:00AM");

			// create a consumer that when it receives a time event, that matches it's schedule, it will trigger a log write event
			var timeEventConsumer = new TimeEventConsumer();
            timeEventConsumer.EventAction = (triggerTime) =>
               {
                   Container.EmitEvent(
                           new LogEvent(Container, timeEventConsumer)
                           {
                               Message = $"Log of: TimeEvent hit at: {triggerTime.Ticks}"
                           }, timeEventConsumer
                       );
               };

			// create a schedule that will only fire the Action when the time matches the event time
			timeEventConsumer.SetSchedule(new Schedule(time));

			// create a consumer that will see the evenets and write to the log
			var logWriteConsumer = new LogEventConsumer();

			Container.RegisterEventConsumer<TimeEvent>(timeEventConsumer);
			Container.RegisterEventConsumer<LogEvent>(logWriteConsumer);

			// run and ensure the listeners are all responding
			Container.Start();

			await Container.EmitEvent(new TimeEvent(Container, time), null);
            await Container.EmitEvent(new TimeEvent(Container, time.AddDays(-10)), null);
            await Container.EmitEvent(new TimeEvent(Container, time.AddDays(10)), null);			
			await Container.Stop();

			Thread.Sleep(500); // just wait a bit for the events to be handled
			

			Container.AssertEventExists<TimeEvent>(3);
			//Container.AssertEventExists<LogEvent>(atLeast: 3);

			Output.WriteLine(Environment.NewLine + "Warehouse Contents:");
			logWriteConsumer.AllLogRecords.ForEach(Output.WriteLine);
			logWriteConsumer.AllLogRecords.Count().Should().Be(4);
		}
	}
}
