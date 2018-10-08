using FluentAssertions;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Tests.IO;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;


namespace Lighthouse.Core.Tests.Hosting
{
	public class NetworkLighthouseServiceContainerConnectionIntegrationTests : LighthouseTestsBase
	{
		public NetworkLighthouseServiceContainerConnectionIntegrationTests(ITestOutputHelper output) : base(output)
		{

		}

		[Fact]
		public async Task TryConnect_UseLoopbackServer()
		{
			var ip = "127.0.0.1";
			var networkProvider = new InternetNetworkProvider(Container);

			var loopBackServer = new TestLocalhostServer(ip);

			// inform the container of this provider
			Container.RegisterResourceProvider(networkProvider);

			var connection = new NetworkLighthouseServiceContainerConnection(Container, System.Net.IPAddress.Parse(ip));
			var found = await connection.TryConnect();
			found.Should().BeFalse();
			connection.ConnectionHistory.Count.Should().Be(1);
			var connectionHistory = connection.ConnectionHistory.Single();

			(connectionHistory.EffectiveDate.Date - DateTime.Now).Seconds.Should().BeLessOrEqualTo(30); // the status time should be in the last 30 seconds, probably much less
			connectionHistory.WasConnected.Should().BeFalse();
			connectionHistory.Exception.Message.Should().Contain("nope");
		}
	}
}
