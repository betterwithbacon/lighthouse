using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Storage;
using NSubstitute;
using System;
using System.Collections.Generic;
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
			var bigPayload = Enumerable.Range(1, 10).Select(i => $"record{i}-{Guid.NewGuid()}").ToArray();
			
			Stopwatch timer = new Stopwatch();

			timer.Start();
			var receipt = warehouse.Store(scope, key, bigPayload);
			var returnedValue = warehouse.Retrieve<string>(scope, key);
			Warehouse.VerifyChecksum(returnedValue, receipt.SHA256Checksum).Should().BeTrue();
			timer.Stop();

			// the amount of time to store, and retrieve a few kilobytes
			output.WriteLine($"Runtime was {Encoding.UTF8.GetByteCount(bigPayload.SelectMany(st => st).ToArray())} bytes in {timer.ElapsedMilliseconds}.");
			timer.ElapsedMilliseconds.Should().BeLessThan(100);
		}

        [Fact]
		[Trait("Category", "Performance")]
		[Trait("Function", "Signing")]
		public void PayloadSigningShouldRoundtripQuickly()
		{
			var payload = new[] { "Test Test test" };
			
			var receipt = warehouse.Store(scope, "test", payload, new[] { StoragePolicy.Ephemeral });

			var returnedValue = warehouse.Retrieve<string>(scope, "test");

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
			var warehouse = new Warehouse();
			var appScope = new ApplicationScope("Test");
			Parallel.ForEach(Enumerable.Range(1, 10),
				new ParallelOptions { MaxDegreeOfParallelism = 10 },
				(index) => {
					var testPayload = index.ToString();					
					warehouse.Store(scope, key, testPayload, new[] { StoragePolicy.Ephemeral });
					output.WriteLine($"Index stored: {index}");
					warehouse.Retrieve<string>(scope, key).Should().Be(testPayload);
				});
		}

		[Fact]
		[Trait("Function", "StoreAndRetrieve")]
		[Trait("Category", "Performance")]
		public void GetManifest_SizeAndPoliciesMatches()
		{
			Assert.False(true);
		}
	}
}
