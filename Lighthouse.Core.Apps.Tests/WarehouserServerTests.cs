using FluentAssertions;
using Lighthouse.Core.Apps.Warehouse;
using System;
using Xunit;
using Xunit.Abstractions;
using WarehouseCore;
using System.Linq;
using Lighthouse.Server;

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
		[Trait("Type","Warehouse")]
        public void ValidService_StartsUpCorrectly()
        {
			var server = new WarehouseServer();
			server.StatusUpdated += Server_StatusUpdated;

			server.Start();

			server.RunState.Should().Be(LighthouseServiceRunState.Running);
		}

		[Fact]
		[Trait("Type", "Warehouse")]
		public void ValidService_DiscoverLocalInMemoryStorage()
		{
			var context = new LighthouseServer(Output.WriteLine);
			context.Start();
			var warehouseServer = new WarehouseServer();
			// bind the warehouserServer to the context
			context.Launch(warehouseServer);

			// get a shelf that can hold data for the duration of the session			
			var remoteShelves = warehouseServer.ResolveShelves(new[] { LoadingDockPolicy.Ephemeral });

			// only one shelf should show up, nd it should be a memshelf
			remoteShelves.OfType<MemoryShelf>().Count().Should().Be(1);

			// no exceptions should have been thrown
			context.GetRunningServices().Where(lsr => lsr.Exceptions.Count > 0).Should().BeEmpty();
		}

		[Fact]
		[Trait("Type", "Warehouse")]
		public void ValidService_StoreAndRetrieve()
		{
			var context = new LighthouseServer(Output.WriteLine);
			context.Start();
			var warehouseServer = new WarehouseServer();
			// bind the warehouserServer to the context
			context.Launch(warehouseServer);

			// get a shelf that can hold data for the duration of the session	
			var payload = new[] { "data" };
			warehouseServer.Store("testData", StorageScope.Global, payload, new[] { LoadingDockPolicy.Ephemeral });

			var retrievedValues = warehouseServer.Retrieve("test", StorageScope.Global);
			payload.Should().BeEquivalentTo(payload);
		}

		private void Server_StatusUpdated(ILighthouseComponent owner, string status)
		{
			Output.WriteLine($"{owner}:{status}");
		}
	}
}
