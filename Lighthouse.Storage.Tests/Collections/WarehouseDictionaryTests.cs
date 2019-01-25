using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Storage;
using Lighthouse.Storage.Collections;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Storage.Tests.Collections
{
	public class WarehouseDictionaryTests
	{
		private readonly ITestOutputHelper output;
		private WarehouseDictionary<string, string> dictionary;
		private StorageKeyManifest expectedManifest;
		private readonly IWarehouse warehouse;
		private readonly IStorageScope testScope;
		private readonly Dictionary<string, string> actualDictionary = new Dictionary<string, string>();

		public WarehouseDictionaryTests(ITestOutputHelper output)
		{
			this.output = output;
			warehouse = Substitute.For<IWarehouse>();
			testScope = new StorageScope("test");
		}

		[Fact]
		public void Insertion_RecordIsInserted()
		{
			GivenStartingDictionary();			
			dictionary.Add("test", "testValue");
			dictionary["test"].Should().Be("testValue");
		}

		[Fact]
		public void Indexer_PreviouslyInsertedRecordsAreThere()
		{
			var guid = Guid.NewGuid().ToString();
			actualDictionary.Add("test", guid);
			GivenStartingDictionary();
			dictionary["test"].Should().Be(guid);
		}

		[Fact]
		public void Add_RecordPushedToPermanentStorage()
		{
			GivenStartingDictionary();			
			dictionary.Add("test", SomeData);			
			ThenAppendIsCalled();			
		}

		[Fact]
		public void Indexer_Add_RecordPushedToPermanentStorage()
		{
			GivenStartingDictionary();			
			dictionary["test"] = SomeData;
			ThenAppendIsCalled();
		}

		[Fact]
		public void Clear_Add_RecordPushedToPermanentStorage()
		{
			GivenStartingDictionary();
			WithExpectedManifest();
			dictionary.Clear();
			dictionary.Count.Should().Be(0);			
			ThenStoreIsCalled();
		}

		#region Assertions
		private readonly string SomeData = Guid.NewGuid().ToString();

		private void ThenAppendIsCalled()
		{
			Thread.Sleep(200);			
			warehouse.ReceivedWithAnyArgs(1).Append(Arg.Any<StorageKey>(), Arg.Any<KeyValuePair<string, string>>());
		}

		private void ThenStoreIsCalled()
		{
			Thread.Sleep(500);
			warehouse.ReceivedWithAnyArgs(1).Store<IDictionary<string, string>>(
				Arg.Any<StorageKey>(),
				Arg.Any<IDictionary<string, string>>(),
				Arg.Any<IEnumerable<StoragePolicy>>());
		}

		void WithExpectedManifest()
		{
			expectedManifest = new StorageKeyManifest
			{
				StoragePolicies = new[] { StoragePolicy.Ephemeral } 
			};
			warehouse.GetManifest(Arg.Any<StorageKey>()).Returns(expectedManifest);
		}

		void GivenStartingDictionary()
		{
			warehouse.Retrieve<Dictionary<string, string>>(Arg.Any<StorageKey>()).ReturnsForAnyArgs(actualDictionary);			
			dictionary = new WarehouseDictionary<string, string>(warehouse, testScope, "testDictionary");
		}
		#endregion
	}

	public class WarehouseDictionaryIntegrationTests
	{
		private readonly ITestOutputHelper output;
		private WarehouseDictionary<string, string> dictionary;
		private StorageKeyManifest expectedManifest;
		private readonly Warehouse warehouse;
		private readonly IStorageScope testScope;
		private readonly Dictionary<string, string> actualDictionary = new Dictionary<string, string>();
		private readonly ILighthouseServiceContainer container;


		public WarehouseDictionaryIntegrationTests(ITestOutputHelper output)
		{
			this.output = output;
			container = Substitute.For<ILighthouseServiceContainer>();
			container.Do(Arg.Do<Action<ILighthouseServiceContainer>>(thingToDo => thingToDo(container)));

			warehouse = new Warehouse();
			warehouse.Initialize(container);
			testScope = new StorageScope("test");			
		}

		[Fact]
		public void Add_DataStoredInActualWarehouse()
		{
			GivenStartingDictionary();
			dictionary.Add("test", "test");
			Thread.Sleep(100); // give the dictionary some time to write to the warehouse
			var warehouseDictionary = warehouse.Retrieve<Dictionary<string, string>>(dictionary.StorageKey);
			warehouseDictionary.Should().NotBeNull();
		}

		void GivenStartingDictionary()
		{
			dictionary = new WarehouseDictionary<string, string>(warehouse, testScope, "testDictionary");
		}
	}
}
