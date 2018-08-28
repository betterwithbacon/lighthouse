using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Deployment;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Server.Tests
{
	public class AppLoadingTests
	{
		private readonly ITestOutputHelper Output;

		public AppLoadingTests(ITestOutputHelper output)
		{
			Output = output;
		}

		[Fact]
		[Trait("Type", "Deployment")]
		public void LighthouseFileSystemLocation_DirectoryLoading_FoundApps()
		{
			var servicesToRun = LighthouseLauncher
				.FindServices(new ILighthouseAppLocation[]
					{
						// TODO: i want to abstract away any lighthouse component having direct file system awareness OR environment variables to faciliate cross-platform support better
						new LighthouseFileSystemLocation(null) { Directory = $"{Environment.CurrentDirectory}\\Apps" }
					}, logHandler: (o, m) => Output.WriteLine($"{o}: {m}")
				);

			servicesToRun.Count().Should().Be(1);
		}

		[Fact]
		[Trait("Type", "Deployment")]
		public void LaunchApp_AppRuns_ThenMessagesWereRecorded()
		{
			// create a service that simply emits a time log even every 10 milliseconds.

			var servicesToStart = new[] {
				new LighthouseAppLaunchConfig
				{
					Id = Guid.NewGuid(),
					Name= "TestApp",
					Type = typeof(TimerApp)
				}
			};

			var messages = new ConcurrentBag<string>();
			var server = new LighthouseServer((message) => { messages.Add(message); Output.WriteLine(message); });

			Output.WriteLine($"Testing started: { DateTime.Now.ToString("mm:fff") }");
			server.Start();
			server.Launch(servicesToStart);

			// just wait some time
			Thread.Sleep(50);			
			Output.WriteLine($"Testing ended: { DateTime.Now.ToString("mm:fff") }");

			// only 3 actual events are emitted in the 50ms window. Due to the vagaries and non-deterministic nature of the Timer.Elapsed method
			messages.Count.Should().BeGreaterOrEqualTo(5);
		}

		[Fact]
		[Trait("Type", "Deployment")]
		public void LaunchApp_AppRuns_ThenServiceStops_AndAppStops()
		{
			// create a service that simply emits a time log even every 10 milliseconds.
			var servicesToStart = new[] {
				new LighthouseAppLaunchConfig
				{
					Id = Guid.NewGuid(),
					Name= "TestApp",
					Type = typeof(TimerApp)
				}
			};

			var server = new LighthouseServer(Output.WriteLine);
			server.Start();
			server.Launch(servicesToStart);

			// just wait some time
			Thread.Sleep(100);

			var runningServices = server.GetRunningServices().ToList();

			runningServices.Count.Should().Be(1);

			// this should wait for graceful exit. and THEN, it sdhould kill them
			server.Stop();

			server.GetRunningServices().Count().Should().Be(0, because: "The services shoudld be stopped.");
		}

		[Fact]
		[Trait("Type", "Deployment")]
		public void LaunchApp_Spawning10Apps_AndTheyAllWork()
		{
			int totalNumberOfServices = 10;

			// create a service that simply emits a time log even every 10 milliseconds.
			var servicesToStart = Enumerable.Range(1, totalNumberOfServices).Select( (_) => 
				new LighthouseAppLaunchConfig
				{
					Id = Guid.NewGuid(),
					Name= "TestApp",
					Type = typeof(TimerApp)
				}
			);

			var server = new LighthouseServer(Output.WriteLine);
			server.Start();
			server.Launch(servicesToStart);

			// just wait some time
			Thread.Sleep(25);

			var runningServices = server.GetRunningServices().ToList();

			runningServices.Count.Should().Be(totalNumberOfServices);

			// this should wait for graceful exit. and THEN, it sdhould kill them
			server.Stop();

			server.GetRunningServices().Count().Should().Be(0, because: "The services shoudld be stopped.");
		}

		private class TimerApp : LighthouseServiceBase
		{
			private System.Timers.Timer Timer { get; set; }

			public TimerApp() // we don't have ways to bootstrap apps with launcher, to the constructor arguments
			{
				Timer = new System.Timers.Timer(10);
				Timer.Elapsed += (o, e) => LighthouseContainer.Log(Core.Logging.LogLevel.Info, this, "event" + DateTime.Now.ToString("ss:fff"));
			}

			protected override void OnStart()
			{				
				Timer.Start();
			}

			protected override void OnStop()
			{
				Timer.Stop();
			}
		}
	}
}
