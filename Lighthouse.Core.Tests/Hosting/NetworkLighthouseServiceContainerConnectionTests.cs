using FluentAssertions;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Management;
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
	public class NetworkLighthouseServiceContainerConnectionTests : LighthouseTestsBase
	{	
		public NetworkLighthouseServiceContainerConnectionTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task TryConnect_ValidResponse_ResponseIsTrue()
		{
			var ip = "127.0.0.1";
			var mockNetworkProvider = Substitute.For<INetworkProvider>();
			mockNetworkProvider.SupportedProtocols.Returns(new[] { NetworkProtocol.HTTP });
			mockNetworkProvider.SupportedScopes.Returns(new[] { NetworkScope.Local });
            mockNetworkProvider
                .GetObjectAsync<LighthouseServerStatus>(Arg.Any<Uri>())
                .Returns((LighthouseServerStatus)null);

            // inform the container of this provider
            Container.RegisterResourceProvider(mockNetworkProvider);

			var connection = new NetworkLighthouseServiceContainerConnection(Container, System.Net.IPAddress.Parse(ip));

			var found = await connection.TryConnect();
			found.Should().BeTrue();

			// verify the connection history
			connection.ConnectionHistory.Count.Should().Be(1);
			var connectionHistory = connection.ConnectionHistory.Single();

			(connectionHistory.EffectiveDate.Date - DateTime.Now).Seconds.Should().BeLessOrEqualTo(30); // the status time should be in the last 30 seconds, probably much less
			connectionHistory.WasConnected.Should().BeTrue();
			connectionHistory.Exception.Should().BeNull();
		}

		[Fact]
		public async Task TryConnect_InvalidResponse_ResponseIsFalse_HistoryHasFailedAttempt()
		{
			var ip = "127.0.0.1";
			var mockNetworkProvider = Substitute.For<INetworkProvider>();
			mockNetworkProvider.SupportedProtocols.Returns(new[] { NetworkProtocol.HTTP });
			mockNetworkProvider.SupportedScopes.Returns(new[] { NetworkScope.Local });
			mockNetworkProvider
                .GetObjectAsync<LighthouseServerStatus>(Arg.Any<Uri>())
                .Returns( (LighthouseServerStatus)null);

			// inform the container of this provider
			Container.RegisterResourceProvider(mockNetworkProvider);

			var connection = new NetworkLighthouseServiceContainerConnection(Container, System.Net.IPAddress.Parse(ip));
			var found = await connection.TryConnect();			
			found.Should().BeFalse();
			connection.ConnectionHistory.Count.Should().Be(1);
			var connectionHistory = connection.ConnectionHistory.Single();

			(connectionHistory.EffectiveDate.Date - DateTime.Now).Seconds.Should().BeLessOrEqualTo(30); // the status time should be in the last 30 seconds, probably much less
			connectionHistory.WasConnected.Should().BeFalse();
			connectionHistory.Exception.Message.Should().Contain("Could not contact server");
		}
	}
}
