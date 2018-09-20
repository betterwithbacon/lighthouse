using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Configuration.Providers;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Events;
using Lighthouse.Core.Events.Queueing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Server.Tests
{
    public class LighthouseServerTests
    {
		protected readonly ITestOutputHelper Output;
		protected readonly ConcurrentBag<string> ContainerMessages = new ConcurrentBag<string>();
		protected LighthouseServer Container { get; private set; }

		public LighthouseServerTests(ITestOutputHelper output)
		{
			this.Output = output;
		}

		protected LighthouseServer GivenAContainer(IWorkQueue<IEvent> workQueue = null,
			double defaultScheduleTimeIntervalInMilliseconds = LighthouseServer.DEFAULT_SCHEDULE_TIME_INTERVAL_IN_MS,
			IAppConfigurationProvider launchConfiguration = null, 
			string workingDirectory = null,
			IWorkQueue<IEvent> eventQueue = null)
		{
			Container = new LighthouseServer(
				(m) =>
				{
					ContainerMessages.Add(m);
					Output.WriteLine(m);
				},
				launchConfiguration: launchConfiguration, 
				workingDirectory: workingDirectory,				
				defaultScheduleTimeIntervalInMilliseconds: defaultScheduleTimeIntervalInMilliseconds,
				eventQueue: workQueue
			);
			return Container;
		}

		#region Logging
		[Fact]
		[Trait("Tag", "Logging")]
		[Trait("Category", "Unit")]
		public void Log_ShouldLog()
		{			
		}
		#endregion

		#region Service Discovery
		[Fact]
		[Trait("Tag", "Logging")]
		[Trait("Category", "Unit")]
		public void FindServices_ShouldFindService()
		{
		}

		[Fact]
		[Trait("Tag", "Logging")]
		[Trait("Category", "Unit")]
		public void FindRemoteServices_ShouldFindService()
		{
		}
		#endregion
		
		#region Utils
		[Fact]
		[Trait("Tag", "Logging")]
		[Trait("Category", "Unit")]
		public void GetTime_ShouldFindDate()
		{
		}
		#endregion

		#region Resource Providers
		[Fact]
		[Trait("Tag", "Resource Providers")]
		[Trait("Category", "Unit")]
		public void GetFileSystemProviders_ShouldFindProvider()
		{
		}

		[Fact]
		[Trait("Tag", "Resource Providers")]
		[Trait("Category", "Unit")]
		public void GetNetworkProviders_ShouldFindProvider()
		{
		}

		[Fact]
		[Trait("Tag", "Resource Providers")]
		[Trait("Category", "Unit")]
		public void WorkingDirectory_ShouldMatchEnvironment()
		{
		}
		#endregion

		#region ComponentModel		
		[Fact]
		[Trait("Tag", "Logging")]
		[Trait("Category", "Unit")]
		public void RegisterComponent_ComponentShouldBeRegistered()
		{
		}
		#endregion

		#region Processing
		[Fact]
		[Trait("Tag", "Processing")]
		[Trait("Category", "Unit")]
		public void Do_ShouldPerformValidOperations()
		{
			var reached = false;

			GivenAContainer().Do((c) => { reached = true; } );

			reached.Should().BeTrue();
		}
		#endregion

		#region Processing
		[Fact]
		[Trait("Tag", "Scheduling")]
		[Trait("Category", "Unit")]
		public void AddScheduledAction_ScheduleAdded()
		{

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
		public TestEvent(ILighthouseServiceContainer container, DateTime? time = null)
		{
			LighthouseContainer = container;
			Time = time ?? DateTime.Now;
		}

		public ILighthouseServiceContainer LighthouseContainer { get; }

		public DateTime Time { get; private set; }
	}

	public static class LighthouseServerConfigurationExtensions
	{
		public static LighthouseServer AssertLaunchConfigurationExists(this LighthouseServer container)
		{
			Assert.NotNull(container.LaunchConfiguration);
			return container;
		}

		public static LighthouseServer AssertLaunchRequestsExists(this LighthouseServer container, Func<ServiceLaunchRequest, bool> filter = null )
		{
			container.AssertLaunchConfigurationExists();
			container.LaunchConfiguration.GetServiceLaunchRequests().Where(slr => filter?.Invoke(slr) ?? true).Should().NotBeEmpty();
			
			return container;
		}
	}
}