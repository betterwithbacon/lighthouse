using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Apps;
using Lighthouse.Core.Events;
using System;
using System.Collections.Concurrent;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Server.Tests
{
    public class TestApp : LighthouseServiceBase
	{
		Action actionToPerform;
		public void SetAction(Action action)
		{
			actionToPerform = action;
		}

		public void PerformAction()
		{
			if (actionToPerform == null)
				throw new InvalidOperationException("No action is set to perform.");

			actionToPerform();
		}
	}

	[Collection("No parallel")]
	public class LighthouseServerTests
    {
		protected readonly ITestOutputHelper Output;
		protected readonly ConcurrentBag<string> ContainerMessages = new ConcurrentBag<string>();
		private LighthouseServer container;
		protected LighthouseServer Container
		{
			get
			{
				if (container == null)
					GivenAContainer();

				return container;
			}
			private set
			{
				container = value;
			}
		}

		public LighthouseServerTests(ITestOutputHelper output)
		{
			this.Output = output;
		}

		protected void GivenAContainer(string workingDirectory = null)
		{
            container = new LighthouseServer();
            container.AddLogger((m) => {
                ContainerMessages.Add(m);
                Output.WriteLine(m);
            });
            container.AddAvailableFileSystemProviders(workingDirectory);
		}

		#region Logging
		[Fact]
		[Trait("Tag", "Logging")]
		[Trait("Category", "Unit")]
		public void Log_ShouldLog()
		{
			var testGuid = Guid.NewGuid().ToString();
			Container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, Container, testGuid);
            Thread.Sleep(25);
			ContainerMessages.Should().Contain((rec) => rec.Contains(testGuid));
		}
		#endregion

		#region Utils
		[Fact]
		[Trait("Tag", "Util")]
		[Trait("Category", "Unit")]
		public void GetTime_ShouldFindDate()
		{
			Container.GetNow().Date.Should().Be(DateTime.Today);
		}

        [Fact] //Skip ="this test is a bit flakey when run with other tests")]
        [Trait("Tag", "Util")]
        [Trait("Category", "Unit")]
        public void ResolveType_ShouldFindType()
        {
            var testEvent = Container.ResolveType<PingService>();
            testEvent.Should().NotBeNull();
        }
        #endregion

        #region Resource Providers
        [Fact]
		[Trait("Tag", "Resource Providers")]
		[Trait("Category", "Unit")]
		public void GetFileSystemProviders_ShouldFindProvider()
		{
            Container.AddAvailableFileSystemProviders();
			Container.GetFileSystemProviders().Should().NotBeEmpty();
		}
		#endregion

		#region Processing
		[Fact]
		[Trait("Tag", "Processing")]
		[Trait("Category", "Unit")]
		public void Do_ShouldPerformValidOperations()
		{
			var reached = false;

			Container.Do((c) => { reached = true; } );
			Thread.Sleep(50);

			reached.Should().BeTrue();
		}
		#endregion

		#region Events
		[Fact]
		[Trait("Tag", "Events")]
		[Trait("Category", "Unit")]
		public void RegisterEventProducer_ProducerAdded()
		{

		}

		[Fact]
		[Trait("Tag", "Events")]
		[Trait("Category", "Unit")]
		public void RegisterEventConsumer_ConsumerAdded()
		{

		}

		[Fact]
		[Trait("Tag", "Events")]
		[Trait("Category", "Unit")]
		public void EmitEvent_EventEmitted()
		{

		}

		[Fact]
		[Trait("Tag", "Events")]
		[Trait("Category", "Unit")]
		public void GetAllReceivedEvents_1EventPrsent_1EventFound()
		{

		}

		[Fact]
		[Trait("Tag", "Events")]
		[Trait("Category", "Unit")]
		public void GetAllReceivedEvents_5EventPrsent_5EventFound()
		{

		}
		#endregion
	}

	public class TestEvent : IEvent
	{
		public TestEvent(ILighthouseServiceContainer Container, DateTime? time = null)
		{
			LighthouseContainer = Container;
			EventTime = time ?? DateTime.Now;
		}

		public ILighthouseServiceContainer LighthouseContainer { get; }

		public DateTime EventTime { get; private set; }
	}
}