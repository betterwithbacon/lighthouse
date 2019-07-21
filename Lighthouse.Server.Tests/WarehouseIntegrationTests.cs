using FluentAssertions;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Storage;
using Lighthouse.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Server.Tests
{
    public class WarehouseIntegrationTests
    {
        private readonly ITestOutputHelper Output;

        public WarehouseIntegrationTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        [Trait("Function", "StoreAndRetrieve")]
        public void OneWarehousePersistsDataIntoAnotherWarehouse()
        {
            // so lighthouse one contains a warehouse
            var server1 = new LighthouseServer(serverName:"server1", localLogger: (m) => Output.WriteLine($"server1: {m}"));
            server1.BindServicePort(50_000);
            server1.Start();

            // another lighthouse contains a warehouse
            var server2 = new LighthouseServer(serverName: "server2", localLogger: (m) => Output.WriteLine($"server2: {m}"));
            server2.BindServicePort(50_001);
            server2.Start();

            var connection1to2 = new LocalLighthouseServiceContainerConnection(server1, server2);
            var connection2to1 = new LocalLighthouseServiceContainerConnection(server2, server1);
            server1.RegisterRemotePeer(connection1to2);
            server2.RegisterRemotePeer(connection2to1);

            // create a warehouse
            var warehouse1 = new Warehouse();
            warehouse1.Initialize(server1);

            var warehouse2 = new Warehouse();
            warehouse2.Initialize(server2);

            var key = "testKey";
            var payload = "payload";

            //these lighthouses only communicate via container connections
            // store something in warehouse1, and it should be replicated in the second warehouse pretty soon
            warehouse1.Store(StorageScope.Global, key, payload, new[] { StoragePolicy.Archival });

            Thread.Sleep(500);

            var retrievedPayload = warehouse2.Retrieve(StorageScope.Global, key);
            payload.Should().Be(retrievedPayload);

        }
    }
}
