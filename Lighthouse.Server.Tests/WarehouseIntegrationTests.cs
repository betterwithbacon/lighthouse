using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Lighthouse.Server.Tests
{
    public class WarehouseIntegrationTests
    {
        [Fact]
        [Trait("Function", "StoreAndRetrieve")]
        public void OneWarehousePersistsDataIntoAnotherWarehouse()
        {
            // so lighthouse one contains a warehouse
            var server1 = new LighthouseServer();
            server1.BindServicePort(50_000);

            // another lighthouse contains a warehouse
            var server2 = new LighthouseServer();
            server2.BindServicePort(50_001);


            //these lighthouses only communicate via TCP
        }
    }
}
