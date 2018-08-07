using FluentAssertions;
using Lighthouse.Core.Deployment;
using System;
using System.Linq;
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
		[Trait("Type","Deployment")]
        public void LighthouseFileSystemLocation_DirectoryLoading_FoundApps()
        {
			var servicesToRun = LighthouseLauncher
				.FindServices(new ILighthouseAppLocation[]
					{
						new LighthouseFileSystemLocation { Directory = $"{Environment.CurrentDirectory}\\Apps" }						
					}, logHandler: (o,m) => Output.WriteLine($"{o}: {m}")
				);

			servicesToRun.Count().Should().Be(1);
		}

		[Fact]
		[Trait("Type", "Deployment")]
		public void DirectoryLoading_FoundApps()
		{
			var servicesToStart = new[] {
				new LighthouseAppLaunchConfig()
			};

			var server = new LighthouseServer(Output.WriteLine);
			server.Launch(servicesToStart);
		}
	}
}
