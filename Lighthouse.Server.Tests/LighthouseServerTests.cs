using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Configuration.Providers;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Events;
using Lighthouse.Core.Events.Queueing;
using Lighthouse.Core.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Lighthouse.Core.Tests.LighthouseServiceTests;

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
			container = new LighthouseServer(
				localLogger: (m) =>
				{
					ContainerMessages.Add(m);
					Output.WriteLine(m);
				},				
				workingDirectory: workingDirectory
			);
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

		[Fact]
		[Trait("Tag", "Resource Providers")]
		[Trait("Category", "Unit")]
		public void GetNetworkProviders_ShouldFindProvider()
		{
            Container.AddAvailableNetworkProviders();
            Container.GetNetworkProviders().Should().NotBeEmpty();
		}

		[Fact]
		[Trait("Tag", "Resource Providers")]
		[Trait("Category", "Unit")]
		public void WorkingDirectory_ShouldMatchEnvironment()
		{
			Container.WorkingDirectory.Should().Be(Environment.CurrentDirectory);
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

		#region Management
		public void SubmitManagementRequest_ManagementType_SubTypeParsed()
		{
			//this.Container.SubmitManagementRequest(
			//	Core.Management.ManagementRequestType.ServerManagement,
				
			//	)
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