//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;
//using Lighthouse.Core;
//using Lighthouse.Core.Configuration.ServiceDiscovery;
//using Lighthouse.Core.Storage.Requests;
//using Lighthouse.Core.Storage.Scopes;
//using Lighthouse.Core.Storage.Stores.Memory;

namespace Lighthouse.Core.Storage.Legacy
{
  //  [ExternalLighthouseService("warehouse")]
  //  public class Warehouse : LighthouseServiceBase, IWarehouse, 
  //      IRequestHandler<KeyValueStoreRequest, AckResponse>,
  //      IRequestHandler<RetrieveRequest, RetrieveResponse>
  //  {
		//// ideally, this will be discovered by reflection
		//public readonly List<Receipt> SessionReceipts = new List<Receipt>();
		//readonly ConcurrentBag<IStore> Stores = new ConcurrentBag<IStore>();
  //      public const int DefaultSyncTimeInMinutes = 60;
  //      // TODO: another way to avoid multiple schedules running simulataneously.
  //      bool isPerformingMaintenance = false;

  //      protected override bool IsInitialized => Stores.Count > 0;

		//protected override void OnInit()
		//{
  //          AddStore(new InMemoryObjectStore());
  //          AddStore(new InMemoryKeyValueStore());
  //      }

		//protected override async Task OnAfterStart()
		//{
		//	// schedule server maintainence to be done each hour
		//	await Container.AddScheduledAction(this, async (time) => await PerformStorageMaintenance(time), DefaultSyncTimeInMinutes);
		//}

		//public async Task<IEnumerable<StorageOperation>> PerformStorageMaintenance(DateTime date)
		//{
  //          var allOperations = new List<StorageOperation>();

  //          if (isPerformingMaintenance)
  //              return allOperations;

  //          isPerformingMaintenance = true;

            
  //          try
  //          {
  //              // perform local sync to stores first                
  //              foreach (var store in Stores.OfType<IKeyValueStore>())
  //              {
  //                  var storeManifest = await store.GetManifests(StorageScope.Global);

  //                  foreach (var item in storeManifest
  //                      .Where(sm =>
  //                          !sm.LastSyncTime.HasValue ||
  //                          sm.LastSyncTime < date.AddMinutes(-1 * DefaultSyncTimeInMinutes)))
  //                  {
  //                      // check the other stores to see if they have this item by the file type

  //                      //TODO: this needs to take into account storage class (ephemeral vs archival)
  //                      foreach (var otherStore in Stores.OfType<IKeyValueStore>().Where(s => s != store))
  //                      {
  //                          var otherItem = (await otherStore.GetManifests(StorageScope.Global, item.Key)).FirstOrDefault();

  //                          if (otherItem == null || otherItem.LastUpdated < item.LastUpdated)
  //                          {
  //                              // store the file
  //                              var payload = store.Retrieve(StorageScope.Global, item.Key);

  //                              otherStore.Store(StorageScope.Global, item.Key, payload);

  //                              allOperations.Add(new StorageOperation { Action=StorageAction.Store, Key= item.Key, Scope=StorageScope.Global });
  //                          }

  //                          item.LastSyncTime = Container.GetNow();
  //                      }
  //                  }
  //              }

  //              TriggerBackgroundSync();
  //          }
  //          finally
  //          {
  //              isPerformingMaintenance = false;
  //          }

  //          return allOperations;
  //      }

  //      public Receipt Store(IStorageScope scope, string key, object data, IEnumerable<StoragePolicy> loadingDockPolicies = null)
  //      {
  //          ThrowIfNotInitialized();

  //          var uuid = Guid.NewGuid();

  //          ConcurrentBag<StoragePolicy> enforcedPolicies = new ConcurrentBag<StoragePolicy>();

  //          if (loadingDockPolicies == null)
  //          {
  //              loadingDockPolicies = new[] { StoragePolicy.Ephemeral };
  //          }

  //          // resolve the appropriate store, based on the policy
  //          Parallel.ForEach(ResolveStores<IObjectStore>(loadingDockPolicies), (shelf) =>
  //          {
  //              shelf.Store(scope, key, data); //, enforcedPolicies);
  //          });

  //          // the receipt is largely what was passed in when it was stored
  //          var receipt = new Receipt(enforcedPolicies.Any())
		//	{
		//		UUID = uuid,
		//		Key = key,
		//		Scope = scope,
		//		// add the policies that were upheld during the store, this is necessary, 
		//		// because this warehouse might not be able to satisfy all of the policies				
		//		Policies = enforcedPolicies.Distinct().ToList(),
		//		SHA256Checksum = CalculateChecksum(data)
		//	};

		//	SessionReceipts.Add(receipt);

		//	return receipt;
		//}

  //      public void AddStore(IStore store)
  //      {
  //          Stores.Add(store);
  //          store.Initialize(this.Container);
  //      }

  //      public Receipt Store(IStorageScope scope, string key, string data, IEnumerable<StoragePolicy> loadingDockPolicies = null)
  //      {
  //          if (loadingDockPolicies == null)
  //          {
  //              loadingDockPolicies = new[] { StoragePolicy.Ephemeral };
  //          }

  //          ConcurrentBag<StoragePolicy> enforcedPolicies = new ConcurrentBag<StoragePolicy>();
  //          Parallel.ForEach(ResolveStores<IKeyValueStore>(loadingDockPolicies), (shelf) =>
  //          {
  //              shelf.Store(scope, key, data);
  //          });

  //          var uuid = Guid.NewGuid();

  //          var receipt = new Receipt(enforcedPolicies.Any())
  //          {
  //              UUID = uuid,
  //              Key = key,
  //              Scope = scope,
  //              // add the policies that were upheld during the store, this is necessary, 
  //              // because this warehouse might not be able to satisfy all of the policies				
  //              Policies = enforcedPolicies.Distinct().ToList(),
  //              SHA256Checksum = CalculateChecksum(data)
  //          };

  //          SessionReceipts.Add(receipt);

  //          //if (syncChangesToOtherWarehouses)
  //          //{
  //          //    TriggerBackgroundSync(key, data);
  //          //}

  //          return receipt;
  //      }

  //      public void TriggerBackgroundSync()
  //      {
  //      }
  //          // for all of the stores, communicate with other warehouses to see if they need the files.
  //          //          var otherContainers = Container.GetServerConnections();
  //          //          var getAllItemsInGlobalScope = new InspectRequest
  //          //          {                
  //          //              Scope = StorageScope.Global
  //          //          };

  //          //          Dictionary<ILighthouseServiceContainerConnection, HashSet<ItemDescriptor>> itemsByContainer = 
  //          //              new Dictionary<ILighthouseServiceContainerConnection, HashSet<ItemDescriptor>>();

  //          //          Dictionary<ILighthouseServiceContainerConnection, HashSet<ItemDescriptor>> itemsToAddContainer =
  //          //              new Dictionary<ILighthouseServiceContainerConnection, HashSet<ItemDescriptor>>();

  //          //          foreach (var containerConnection in otherContainers)
  //          //          {
  //          //              var response = containerConnection.MakeRequest<InspectRequest, InspectResponse>(getAllItemsInGlobalScope);
  //          //              itemsByContainer.Add(containerConnection, response.Items.ToHashSet());
  //          //          }

  //          //          // get all items in current warehouse
  //          //          foreach(var item in GetManifest(StorageScope.Global).Distinct())
  //          //          {
  //          //              foreach(var containerAndItems in itemsByContainer)
  //          //              {
  //          //                  // the remote container does not have the item, so give it to them
  //          //                  if(!containerAndItems.Value.Contains(item))
  //          //                  {
  //          //                      var value = Retrieve(StorageScope.Global, item.Key);

  //          //                      containerAndItems.Key.MakeRequest<KeyValueStoreRequest, AckResponse>(
  //          //                          new KeyValueStoreRequest
  //          //                          {
  //          //                              Scope = StorageScope.Global,
  //          //                              Key = item.Key,      
  //          //                              Value = value
  //          //                          });
  //          //                  }
  //          //              }
  //          //          }
  //          //      }

  //      public IEnumerable<T> ResolveStores<T>(IEnumerable<StoragePolicy> loadingDockPolicies)
  //          where T : IStore
  //      {
  //          return Stores.OfType<T>().Where(s => s.CanEnforcePolicies(loadingDockPolicies));
  //      }

  //      void ThrowIfNotInitialized()
  //      {
  //          if (!IsInitialized)
  //              throw new InvalidOperationException("Warehouse not initialized.");
  //      }

  //      public static string CalculateChecksum(object input)
  //      {
  //          if (input == null)
  //          {
  //              input = string.Empty;
  //          }

  //          // can't calculate checksums for non strings right now
  //          // TODO: add support for non-strings
  //          if (input.GetType() != typeof(string))
  //              return String.Empty;

  //          using (var sha256 = SHA256.Create())
  //          {
  //              byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes(input as string));
  //              var sBuilder = new StringBuilder();
  //              for (int i = 0; i < data.Length; i++)
  //                  sBuilder.Append(data[i].ToString("x2"));

  //              return sBuilder.ToString();
  //          }
  //      }

		//public static bool VerifyChecksum(string input, string hash)
		//{
		//	return CalculateChecksum(input).Equals(hash);
		//}

  //      public T Retrieve<T>(IStorageScope scope, string key)
  //      {
  //          ThrowIfNotInitialized();

  //          if(typeof(T) == typeof(string))
  //          {
  //              // TODO: another hack
  //              throw new InvalidOperationException("Should not store strings with this method. Use the non-generic version for that.");
  //          }

  //          // this is a hack until we can figure out which store to pick (different versions of the same file?)
  //          var objectStore = Stores
  //                  .OfType<IObjectStore>()
  //                  .FirstOrDefault(store => store.CanRetrieve(scope, key));

  //          if (objectStore == null)
  //          {
  //              return default;
  //          }

  //          var val = objectStore.Retrieve<T>(scope, key);

  //          if (val != null)
  //              return val;
  //          else
  //              return default;
  //      }

  //      public string Retrieve(IStorageScope scope, string key)
  //      {
  //          ThrowIfNotInitialized();
  //          // this is a hack until we can figure out which store to pick (different versions of the same file?)
  //          var keyValStore = Stores
  //                  .OfType<IKeyValueStore>()
  //                  .FirstOrDefault(store => store.CanRetrieve(scope, key));

  //          if (keyValStore == null)
  //          {
  //              return default;
  //          }

  //          var val = keyValStore.Retrieve(scope, key);

  //          if (val != null)
  //              return val;
  //          else
  //              return default;
  //      }

  //      public IEnumerable<ItemDescriptor> GetManifest(IStorageScope scope, string key = null)
  //      {
  //          HashSet<ItemDescriptor> items = new HashSet<ItemDescriptor>();
  //          foreach(var store in Stores)
  //          {
  //              foreach(var item in store.GetManifests(scope, key).GetAwaiter().GetResult())
  //              {
  //                  items.Add(item);
  //              }
  //          }

  //          return items;
  //      }

  //      public InspectResponse Handle(InspectRequest request)
  //      {
  //          var items = GetManifest(request.Scope);
            
  //          return new InspectResponse
  //          {
  //              Items = items.ToList()
  //          };
  //      }

  //      public AckResponse Handle(KeyValueStoreRequest request)
  //      {
  //          return new AckResponse
  //          {
  //              Receipt = Store(request.Scope, request.Key, request.Value)
  //          };
  //      }

  //      public RetrieveResponse Handle(RetrieveRequest request)
  //      {
  //          var payload = Retrieve(request.Scope, request.Key);

  //          return new RetrieveResponse
  //          {
  //              Data = payload
  //          };
  //      }
  //  }

  //  public class WarehouseConfig
  //  {
  //      public List<WarehouseConnection> Connections { get; set; }
  //  }

  //  public class WarehouseConnection
  //  {
  //      public WarehouseServerConnectionType Type { get; set; }
  //      public string ConnectionString { get; set; }
  //  }

  //  public enum WarehouseServerConnectionType
  //  {
  //      KeyValue,
  //      Relational,
  //      Blob
  //  }
}
