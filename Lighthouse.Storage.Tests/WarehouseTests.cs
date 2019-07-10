using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Storage;
using NSubstitute;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Storage.Tests
{
    public class WarehouseTests
	{
		private readonly ITestOutputHelper output;
        IStorageScope scope = new ApplicationScope("TestApp");
        string key = "testKey";
        string payload = "testPayload";
        readonly Warehouse warehouse = new Warehouse();
        readonly ILighthouseServiceContainer container;

        public WarehouseTests(ITestOutputHelper output)
		{
			this.output = output;
            container = Substitute.For<ILighthouseServiceContainer>();
            warehouse.Initialize(container);
        }

		[Fact]
		[Trait("Function", "StoreAndRetrieve")]
		public void MemoryWarehouseShouldStoreAndReturnPallet()
		{
            var receipt = warehouse.Store(scope, key, payload, new[] { StoragePolicy.Ephemeral });
            var returnedValue = warehouse.Retrieve(scope, key);
            returnedValue.Should().Be(payload);
		}

		[Fact]
		[Trait("Function", "Signing")]
		public void PayloadSigningShouldRoundtrip()
		{
            var bigPayload = string.Join(',',Enumerable.Range(1, 10).Select(i => $"record{i}-{Guid.NewGuid()}"));
			
			Stopwatch timer = new Stopwatch();

			timer.Start();
			var receipt = warehouse.Store(scope, key, bigPayload);
			var returnedValue = warehouse.Retrieve(scope, key);
			Warehouse.VerifyChecksum(returnedValue, receipt.SHA256Checksum).Should().BeTrue();
			timer.Stop();

			// the amount of time to store, and retrieve a few kilobytes
			output.WriteLine($"Runtime was {Encoding.UTF8.GetByteCount(bigPayload)} bytes in {timer.ElapsedMilliseconds}ms.");
			timer.ElapsedMilliseconds.Should().BeLessThan(100);
		}

        [Fact]
		[Trait("Category", "Performance")]
		[Trait("Function", "Signing")]
		public void PayloadSigningShouldRoundtripQuickly()
		{
			var payload = "Test Test test";
			
			var receipt = warehouse.Store(scope, "test", payload, new[] { StoragePolicy.Ephemeral });

			var returnedValue = warehouse.Retrieve(scope, "test");

			Warehouse.VerifyChecksum(returnedValue, receipt.SHA256Checksum).Should().BeTrue();
		}

		//[Fact]
		//[Trait("Type", "Warehouse")]
		//[Trait("Function", "StoreAndRetrieve")]
		//public void MemoryWarehouseShouldStoreAndReturnAndAppendAndReturnPallet()
		//{	
		//	var receipt = warehouse.Store(scope, key , payload, new[] { StoragePolicy.Ephemeral });

		//	var returnedValue = warehouse.Retrieve<string>(key).ToList();

		//	returnedValue.Should().Contain(payload);

		//	var nextText = " 123456789";
		//	payload += nextText;

		//	warehouse.St(key, new[] { nextText });

		//	var newReturnedValue = warehouse.Retrieve<string>(key);
		//	newReturnedValue.Should().Contain(payload);
		//}

		[Fact]
		[Trait("Function", "StoreAndRetrieve")]
		[Trait("Category", "Performance")]
		[Trait("Facet", "Mult-Threading")]
		public void MultiThreadedWriteReadPerformanceTest()
		{	
            _ = Parallel.ForEach(Enumerable.Range(1, 10),
                new ParallelOptions { MaxDegreeOfParallelism = 10 },
                    (index) => {
                        var threadKey = index.ToString();
                        var testPayload = index.ToString();
                        warehouse.Store(scope, threadKey, testPayload);
                        output.WriteLine($"Index stored: {index}");
                        warehouse.Retrieve(scope, threadKey).Should().Be(testPayload);
                    }
                );
		}

		[Fact]
		[Trait("Function", "StoreAndRetrieve")]
		[Trait("Category", "Performance")]
		public void GetManifest_SizeAndPoliciesMatches()
		{
			
		}

        [Fact]
        [Trait("Function", "StoreAndRetrieve")]        
        public void OneWarehousePersistsDataIntoAnotherWarehouse()
        {
            // so lighthouse one contains a warehouse
            
            // another lighthouse contains a warehouse

            //these lighthouses only communicate via TCP
        }
    }
}
