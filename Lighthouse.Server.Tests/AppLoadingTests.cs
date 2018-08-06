using FluentAssertions;
using Lighthouse.Core.Deployment;
using System;
using System.Linq;
using Xunit;

namespace Lighthouse.Server.Tests
{
    public class AppLoadingTests
    {
        [Fact]
		[Trait("Type","Deployment")]
        public void DirectoryLoading_FoundApps()
        {
			var servicesToRun = LighthouseLauncher
				.FindServices(new ILighthouseAppLocation[]
					{
						new LighthouseFileSystemLocation { Directory = $"{Environment.CurrentDirectory}\\Apps" }						
					}
				);

			servicesToRun.Count().Should().Be(2);
		}
    }
}
