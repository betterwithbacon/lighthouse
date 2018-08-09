using FluentAssertions;
using Lighthouse.Core.Apps.Warehouse;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Core.Apps.Tests
{
    public class WarehouserServerTests
    {
		private readonly ITestOutputHelper Output;

		public WarehouserServerTests(ITestOutputHelper output)
		{
			Output = output;
		}

		[Fact]
        public void ValidService_StartsUpCorrectly()
        {
			var server = new WarehouserServer();
			server.StatusUpdated += Server_StatusUpdated;

			server.Start();

			server.RunState.Should().Be(LighthouseServiceRunState.Running);
		}

		[Fact]
		public void ValidService_DiscoversInMemoryStorage()
		{
			var server = new WarehouserServer();
			server.StatusUpdated += Server_StatusUpdated;
			server.Start();

			var remoteShelves = server.GetShelves();

		}

		private void Server_StatusUpdated(ILighthouseComponent owner, string status)
		{
			Output.WriteLine($"{owner}:{status}");
		}
	}
}
