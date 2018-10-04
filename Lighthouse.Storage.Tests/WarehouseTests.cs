using FluentAssertions;
using Lighthouse.Core.Storage;
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

		public WarehouseTests(ITestOutputHelper output)
		{
			this.output = output;
		}

		[Fact]
		[Trait("Function", "StoreAndRetrieve")]
		public void MemoryWarehouseShouldStoreAndReturnPallet()
		{
			var warehouse = new Warehouse();
			var payload = new[] { "Test Test test" };
			var scope = new ApplicationScope("TestApp");
			var key = new StorageKey($"key", scope);

			var receipt = warehouse.Store(key, payload, new[] { StoragePolicy.Ephemeral });

			var returnedValue = warehouse.Retrieve<string>(key).ToList();

			returnedValue.Should().Contain(payload);
		}

		[Fact]
		[Trait("Function", "Signing")]
		public void PayloadSigningShouldRoundtrip()
		{
			var warehouse = new Warehouse();
			var payload = Enumerable.Range(1, 500).Select(i => $"record{i}-{Guid.NewGuid()}").ToArray();

			var scope = new ApplicationScope("TestApp");
			var key = new StorageKey($"key", scope);

			Stopwatch timer = new Stopwatch();

			timer.Start();
			var receipt = warehouse.Store(key, payload, new[] { StoragePolicy.Ephemeral });
			var returnedValue = warehouse.Retrieve<string>(key).ToList();
			Warehouse.VerifyChecksum(returnedValue, receipt.SHA256Checksum).Should().BeTrue();
			timer.Stop();

			// the amount of time to store, and retrieve a few kilobytes
			output.WriteLine($"Runtime was {Encoding.UTF8.GetByteCount(payload.SelectMany(st => st).ToArray())} bytes in {timer.ElapsedMilliseconds}.");
			timer.ElapsedMilliseconds.Should().BeLessThan(100);
		}

		[Fact]
		[Trait("Category", "Performance")]
		[Trait("Function", "Signing")]
		public void PayloadSigningShouldRoundtripQuickly()
		{
			var warehouse = new Warehouse();
			var payload = new[] { "Test Test test" };
			var scope = new ApplicationScope("TestApp");
			var key = new StorageKey($"key", scope);

			var receipt = warehouse.Store(key, payload, new[] { StoragePolicy.Ephemeral });

			var returnedValue = warehouse.Retrieve<string>(key).ToList();

			Warehouse.VerifyChecksum(returnedValue, receipt.SHA256Checksum).Should().BeTrue();
		}

		[Fact]
		[Trait("Type", "Warehouse")]
		[Trait("Function", "StoreAndRetrieve")]
		public void MemoryWarehouseShouldStoreAndReturnAndAppendAndReturnPallet()
		{
			var warehouse = new Warehouse();
			var payload = new List<string>() { "Test Test test" };
			var scope = new ApplicationScope("TestApp");
			var key = new StorageKey($"key", scope);

			var receipt = warehouse.Store(key, payload, new[] { StoragePolicy.Ephemeral });

			var returnedValue = warehouse.Retrieve<string>(key).ToList();

			returnedValue.Should().Contain(payload);

			var nextText = " 123456789";
			payload.Add(nextText);

			warehouse.Append(key, new[] { nextText }, receipt.Policies);

			var newReturnedValue = warehouse.Retrieve<string>(key);
			newReturnedValue.Should().Contain(payload);
		}

		[Fact]
		[Trait("Function", "StoreAndRetrieve")]
		[Trait("Category", "Performance")]
		[Trait("Facet", "Mult-Threading")]
		public void MultiThreadedWriteReadPerformanceTest()
		{
			var warehouse = new Warehouse();
			var appScope = new ApplicationScope("Test");
			Parallel.ForEach(Enumerable.Range(1, 100),
				new ParallelOptions { MaxDegreeOfParallelism = 10 },
				(index) => {
					var payload = new[] { index.ToString() };
					var key = new StorageKey($"key_{index}", appScope);
					warehouse.Store(key, payload, new[] { StoragePolicy.Ephemeral });
					output.WriteLine($"Index stored: {index}");
					warehouse.Retrieve<string>(key).Should().Contain(payload);
				});
		}

		[Fact]
		[Trait("Function", "StoreAndRetrieve")]
		[Trait("Category", "Performance")]
		[Trait("Facet", "Mult-Threading")]
		public void MultiThreadedAppendReadPerformanceTest()
		{
			var warehouse = new Warehouse();
			var appScope = new ApplicationScope("Test");

			Parallel.ForEach(Enumerable.Range(1, 100),
				new ParallelOptions { MaxDegreeOfParallelism = 10 },
				(index) => {
					var payload = new[] { index.ToString() };
					var additionalPayload = new[] { (index + 100).ToString() };
					var key = new StorageKey($"key_{index}", appScope);
					warehouse.Store(key, payload, new[] { StoragePolicy.Ephemeral });
					warehouse.Append(key, additionalPayload, new[] { StoragePolicy.Ephemeral });
					output.WriteLine($"Index stored: {index}");
					warehouse.Retrieve<string>(key).Should().Contain(payload.Concat(additionalPayload));
				});
		}

		[Fact]
		[Trait("Function", "StoreAndRetrieve")]
		[Trait("Category", "Performance")]
		public void MultiThreadedAppendReadToOneKeyPerformanceTest()
		{
			var warehouse = new Warehouse();
			var appScope = new ApplicationScope("Test");
			var key = new StorageKey("key", appScope);
			var payload = new[] { "initial" };
			warehouse.Store(key, payload, new[] { StoragePolicy.Ephemeral });

			Parallel.ForEach(Enumerable.Range(1, 100),
				new ParallelOptions { MaxDegreeOfParallelism = 10 },
				(index) => {
					warehouse.Append(key, new[] { (index + 100).ToString() }, new[] { StoragePolicy.Ephemeral });
					output.WriteLine($"Index stored: {index}");

				});

			warehouse.Retrieve<string>(key).Count().Should().Be(101);
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
