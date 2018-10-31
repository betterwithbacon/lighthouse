﻿using FluentAssertions;
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

		protected void GivenAContainer(
			IWorkQueue<IEvent> workQueue = null,
			double defaultScheduleTimeIntervalInMilliseconds = LighthouseServer.DEFAULT_SCHEDULE_TIME_INTERVAL_IN_MS,
			IAppConfigurationProvider launchConfiguration = null, 
			string workingDirectory = null,
			IWorkQueue<IEvent> eventQueue = null)
		{
			container = new LighthouseServer(
				localLogger: (m) =>
				{
					ContainerMessages.Add(m);
					Output.WriteLine(m);
				},				
				workingDirectory: workingDirectory
			);

			if(defaultScheduleTimeIntervalInMilliseconds != LighthouseServer.DEFAULT_SCHEDULE_TIME_INTERVAL_IN_MS)
				container.OverrideGlobalClock(fireEvery:defaultScheduleTimeIntervalInMilliseconds);

			if(eventQueue != null)
				container.AddEventQueue(eventQueue);
		}

		#region Logging
		[Fact]
		[Trait("Tag", "Logging")]
		[Trait("Category", "Unit")]
		public void Log_ShouldLog()
		{
			var testGuid = Guid.NewGuid().ToString();
			Container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, Container, testGuid);
			ContainerMessages.Should().Contain((rec) => rec.Contains(testGuid));
		}
		#endregion

		#region Service Discovery
		[Fact]
		[Trait("Tag", "Logging")]
		[Trait("Category", "Unit")]
		public void FindServices_ShouldFindService()
		{	
			Container.Start();
			Container.Launch(new TestApp());
			var foundTestApp = Container.FindServices<TestApp>();
			foundTestApp.Should().NotBeEmpty();
		}

		[Fact]
		[Trait("Tag", "ServiceDiscovery")]
		[Trait("Category", "Unit")]
		public async Task FindRemoteServices_ShouldFindService()
		{			
			var otherContainer = new LighthouseServer(serverName: "Lighthouse Server #2", localLogger:(message) => Output.WriteLine($"Lighthouse Server #2: {message}"));

			// inform the first Container about the other
			Container.RegisterRemotePeer(new LocalLighthouseServiceContainerConnection(Container,otherContainer));

			// start both Containers
			Container.Start();
			otherContainer.Start();

			// launch the app in the "other"
			otherContainer.Launch(new TestApp());

			// the local Container should be able to find the service running in the other one
			var foundTestApp = await Container.FindRemoteServices<TestApp>();
			foundTestApp.Should().NotBeEmpty();
		}

		[Fact]
		[Trait("Tag", "ServiceDiscovery")]
		[Trait("Category", "Unit")]
		public async Task FindRemoteService_LocalConnection_ShouldProxyCommandsCorrectly()
		{
			var otherContainer = new LighthouseServer(serverName: "Lighthouse Server #2", localLogger: (message) => Output.WriteLine($"Lighthouse Server #2: {message}"));
			// inform the first Container about the other
			Container.RegisterRemotePeer(new LocalLighthouseServiceContainerConnection(Container, otherContainer));

			// start both Containers
			Container.Start();
			otherContainer.Start();

			// launch the app in the "other" container
			otherContainer.Launch(typeof(TestApp));
			bool hit = false;

			var originalApp = otherContainer.FindServices<TestApp>().First();

			// configure what this program will "do"
			originalApp.SetAction(() => { hit = true; originalApp.Container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info,originalApp, "This action was hit"); } );

			// the local Container should be able to find the service running in the other one
			var foundTestApp = await Container.FindRemoteServices<TestApp>();
			foundTestApp.Should().NotBeEmpty();
			var proxy = foundTestApp.Single();

			// so in this case, the proxy should wrap the call, and perform it on the remote target
			proxy.Service.PerformAction();

			// this means that the proxy 
			hit.Should().BeTrue();
		}

		[Fact]
		[Trait("Tag", "ServiceDiscovery")]
		[Trait("Category", "Unit")]
		public async Task FindRemoteService_NetworkConnection_ShouldProxyCommandsCorrectly()
		{
			var serverPort = 54546;
			Container.AddAvailableNetworkProviders();

			var otherContainer = new LighthouseServer(
				serverName: "Lighthouse Server #2", localLogger: (message) => Output.WriteLine($"Lighthouse Server #2: {message}"),
				enableManagementService: true
			);
			otherContainer.AddAvailableNetworkProviders();
			otherContainer.AddHttpManagementInterface();

			// this server will listen on this port for other lighthouse containers
			otherContainer.BindServicePort(serverPort);

			var otherContainerAddress = $"127.0.0.1";

			// inform the first Container about the other
			Container.RegisterRemotePeer(
				new NetworkLighthouseServiceContainerConnection(Container, IPAddress.Parse(otherContainerAddress)));

			// start both Containers
			Container.Start();
			otherContainer.Start();

			// launch the app in the "other" container
			otherContainer.Launch(typeof(TestApp));
			bool hit = false;

			var originalApp = otherContainer.FindServices<TestApp>().First();

			// configure what this program will "do"
			originalApp.SetAction(() => { hit = true; originalApp.Container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, originalApp, "This action was hit"); });

			// the local Container should be able to find the service running in the other one
			var foundTestApp = await Container.FindRemoteServices<TestApp>();
			foundTestApp.Should().NotBeEmpty();
			var proxy = foundTestApp.Single();

			// so in this case, the proxy should wrap the call, and perform it on the remote target
			proxy.Service.PerformAction();

			// this means that the proxy 
			hit.Should().BeTrue();
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
			Container.GetFileSystemProviders().Should().NotBeEmpty();
		}

		[Fact]
		[Trait("Tag", "Resource Providers")]
		[Trait("Category", "Unit")]
		public void GetNetworkProviders_ShouldFindProvider()
		{
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

			Container.Do((c) => { reached = true; } );
			Thread.Sleep(50);

			reached.Should().BeTrue();
		}
		#endregion

		#region Processing
		[Fact]
		[Trait("Tag", "Scheduling")]
		[Trait("Category", "Unit")]
		public void AddScheduledAction_ScheduleAdded()
		{
			Container.AddScheduledAction(new Core.Scheduling.Schedule(Core.Scheduling.ScheduleFrequency.Hourly, 1, "Test"), (_) => { });
			Container.GetSchedules().Where(schedule => schedule.Name == "Test").Should().NotBeEmpty();
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
			Time = time ?? DateTime.Now;
		}

		public ILighthouseServiceContainer LighthouseContainer { get; }

		public DateTime Time { get; private set; }
	}

	public static class LighthouseServerConfigurationExtensions
	{
		public static LighthouseServer AssertLaunchConfigurationExists(this LighthouseServer Container)
		{
			//Assert.NotNull(Container.LaunchConfiguration);
			return Container;
		}

		public static LighthouseServer AssertLaunchRequestsExists(this LighthouseServer Container, Func<ServiceLaunchRequest, bool> filter = null )
		{
			Container.AssertLaunchConfigurationExists();
			Container.ServiceLaunchRequests.Where(slr => filter?.Invoke(slr) ?? true).Should().NotBeEmpty();
			
			return Container;
		}
	}
}