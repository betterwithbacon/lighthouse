using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Lighthouse.Server.Tests
{
    public class LighthouseServerTests
    {
		[Fact]
		[Trait("Type", "Deployment")]
		public void FindServices_DirectoryLoading_FoundApps()
		{
			//var servicesToRun = LighthouseLauncher
			//	.FindServices(new ILighthouseAppLocation[]
			//		{
			//			new LighthouseFileSystemLocation { Directory = $"{Environment.CurrentDirectory}\\Apps" }
			//		}, logHandler: (o, m) => Output.WriteLine($"{o}: {m}")
			//	);

			//servicesToRun.Count().Should().Be(1);
		}
	}
}
