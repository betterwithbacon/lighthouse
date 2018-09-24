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
using Lighthouse.Core.Events;
using Lighthouse.Core.Tests.Events;

namespace Lighthouse.Server.Tests.Performance
{
	public class PerformanceTests : LighthouseServerTests
	{
		public PerformanceTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		[Trait("Category", "Performance")]
		public async Task PerformanceTest_HundredsOfHeterogeneousEventsFinishesInAFewSeconds()
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

			var totalEventsSent = 10_000;

			// create a consumer that will see the evenets and write to the log
			var logWriteConsumer = new LogEventConsumer();

			container.RegisterEventConsumer<TimeEvent>(timeEventConsumer);
			container.RegisterEventConsumer<LogEvent>(logWriteConsumer);

			// run and ensure the listeners are all responding
			container.Start();

			var stopwatch = new Stopwatch();

			var overheadTime = 0m;
			stopwatch.Start();

			Parallel.ForEach(Enumerable.Range(1, totalEventsSent), new ParallelOptions { MaxDegreeOfParallelism = 10 },
				(index) => { } //container.EmitEvent(new TimeEvent(container, time.AddDays(-1 * index)), null)				
			);

			stopwatch.Stop();

			overheadTime = stopwatch.ElapsedMilliseconds;

			// see how long it takes for the events to be emitted and then responded too
			stopwatch.Restart();

			var tasks = Enumerable.Range(1, totalEventsSent)
				.Select(index => Task.Run(() => container.EmitEvent(new TimeEvent(container, time.AddDays(-1 * index)), null)));

			await Task.WhenAll(tasks);

			stopwatch.Stop();

			var netRunTime = stopwatch.ElapsedMilliseconds - overheadTime;

			Output.WriteLine($"Total Events: totalEventsSent, Total runtime: {stopwatch.ElapsedMilliseconds}, Runtime: {netRunTime}ms. Avg Event Time: {(netRunTime / totalEventsSent).ToString("G")} ms/event. Events/Second: {(totalEventsSent / netRunTime).ToString("0.0000")} events/ms");

			// it should be able to process a hundred events in under a second
			stopwatch.ElapsedMilliseconds.Should().BeLessThan(totalEventsSent * (long)2); // each event should never take longer trhan 1.5milliseconds to run
		}

		[Fact]
		[Trait("Category", "Performance")]
		public async Task PerformanceTest_HundredsOfTimingEventsFinishesInAFewSeconds()
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

			var totalEventsSent = 100;

			// create a consumer that will see the evenets and write to the log
			var logWriteConsumer = new LogEventConsumer();

			container.RegisterEventConsumer<TimeEvent>(timeEventConsumer);
			container.RegisterEventConsumer<LogEvent>(logWriteConsumer);

			// run and ensure the listeners are all responding
			container.Start();

			var tasks = Enumerable.Range(1, totalEventsSent)
				.Select(index => Task.Run(() => container.EmitEvent(new TimeEvent(container, time.AddDays(-1 * index)), null)));

			await Task.WhenAll(tasks);

			container.AssertEventExists<TimeEvent>(totalEventsSent);
			container.AssertEventExists<LogEvent>(totalEventsSent);

			Output.WriteLine(Environment.NewLine + "Warehouse Contents:");

			Parallel.ForEach(logWriteConsumer.AllLogRecords,
				new ParallelOptions { MaxDegreeOfParallelism = 10 },
				(line) =>
				{
					Output.WriteLine(line);
				}
			);

			logWriteConsumer.AllLogRecords.Count().Should().Be(totalEventsSent + 1);
		}
	}
}
