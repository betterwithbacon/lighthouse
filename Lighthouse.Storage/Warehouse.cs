using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Storage;
using Lighthouse.Storage.Memory;

namespace Lighthouse.Storage
{
    [ExternalLighthouseService("warehouse")]
    public class Warehouse : LighthouseServiceBase, IWarehouse, IRequestHandler<StorageRequest,StorageResponse>
	{
		// TODO: this is atemp solution to this problem of discoverying available shelf types
		// ideally, this will be discovered by reflection		
		public readonly List<Receipt> SessionReceipts = new List<Receipt>();
		readonly ConcurrentBag<IStore> Stores = new ConcurrentBag<IStore>();
		
		protected override bool IsInitialized => Stores.Count > 0;

		protected override void OnInit()
		{
			// Discover shelf types
			foreach (var store in DiscoverStores())
			{
				Stores.Add(store);

				// create all the shelves in the global scope
				store.Initialize(this.Container);
			}
		}

		protected override void OnAfterStart()
		{
			// schedule server maintainence to be done each hour
			Container.AddScheduledAction(this, async (time) => { await PerformStorageMaintenance(time); }, 60);

			//// populate the remote warehouses			
			//LoadRemoteWarehouses().RunSynchronously();
		}

		public IEnumerable<IStore> DiscoverStores()
		{
			yield return new InMemoryObjectStore();
            yield return new InMemoryKeyValueStore();

            //foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) //.SelectMany(a => a.GetTypes()).Where(t => typeof(IShelf).IsAssignableFrom(t) && t.IsClass))
            //{
            //	foreach(var type in assembly.GetTypes())
            //	{
            //		bool isCorrect = false;

            //		try
            //		{
            //			if (typeof(IShelf).IsAssignableFrom(type) && type.IsClass)
            //			{
            //				isCorrect = true;
            //			}
            //		}
            //		catch (Exception) { }

            //		if(isCorrect)
            //			yield return Activator.CreateInstance(type) as IShelf;
            //	}
            //	//yield return Activator.CreateInstance(type) as IShelf;
            //}
        }

		//private async Task LoadRemoteWarehouses()
		//{
		//	// the container is how remote lighthouse resources are found
		//	if (Container != null)
		//	{
		//		Container.Log(Lighthouse.Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, this, "Loading remote warehouses.");

		//		// the Lighthouse context should know about the other services that are running
		//		foreach (var remoteWarehouse in Container.FindServices<Warehouse>())
		//		{
		//			// skip THIS service.
		//			if (remoteWarehouse.Id == this.Id)
		//				continue;

		//			RemoteWarehouses.Add(remoteWarehouse);
		//			Container.Log(Lighthouse.Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, this, $"Container local warehouse {remoteWarehouse} was added.");
		//		}

		//		// this is where an network discovery will occur. to reach other points, not local to this lighthouse runtime.
		//		// currently, this isn't implemented, but ideally
		//		foreach (var remoteWarehouseProxy in await Container.FindRemoteServices<Warehouse>())
		//		{
		//			//// skip THIS service.
		//			//if (remoteWarehouseProxy.Service.Id == this.Id)
		//			//	continue;

		//			//RemoteWarehouses.Add(remoteWarehouse);
		//			//LighthouseContainer.Log(Lighthouse.Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, this, $"Remote warehouse {remoteWarehouse} was added.");
		//		}
		//	}
		//}
		
		public async Task<IEnumerable<StorageOperation>> PerformStorageMaintenance(DateTime date)
		{
            // for each store
            // get all of the manifests
            // and for each item in the manifest, look to see if the item should be moved in some way
            foreach(var store in Stores)
            {
                var storeManifest = await store.GetManifest(StorageScope.Global);

                foreach (var item in await storeManifest.GetItems())
                {

                }
            }

            return Enumerable.Empty<StorageOperation>();
		}

        //public Receipt Store<T>(StorageKey key, T data, IEnumerable<StoragePolicy> loadingDockPolicies)
        public Receipt Store(IStorageScope scope, string key, object data, IEnumerable<StoragePolicy> loadingDockPolicies = null)
        {
            ThrowIfNotInitialized();

            var uuid = Guid.NewGuid();

            ConcurrentBag<StoragePolicy> enforcedPolicies = new ConcurrentBag<StoragePolicy>();

            if (loadingDockPolicies == null)
            {
                loadingDockPolicies = new[] { StoragePolicy.Ephemeral };
            }

            // resolve the appropriate store, based on the policy
            Parallel.ForEach(ResolveShelves<IObjectStore>(loadingDockPolicies), (shelf) =>
            {
                shelf.Store(scope, key, data, enforcedPolicies);
            });

            // the receipt is largely what was passed in when it was stored
            var receipt = new Receipt(enforcedPolicies.Any())
			{
				UUID = uuid,
				Key = key,
				Scope = scope,
				// add the policies that were upheld during the store, this is necessary, 
				// because this warehouse might not be able to satisfy all of the policies				
				Policies = enforcedPolicies.Distinct().ToList(),
				SHA256Checksum = CalculateChecksum(data)
			};

			SessionReceipts.Add(receipt);

			return receipt;
		}

        public Receipt Store(IStorageScope scope, string key, string data, IEnumerable<StoragePolicy> loadingDockPolicies = null)
        {
            if (loadingDockPolicies == null)
            {
                loadingDockPolicies = new[] { StoragePolicy.Ephemeral };
            }

            ConcurrentBag<StoragePolicy> enforcedPolicies = new ConcurrentBag<StoragePolicy>();
            Parallel.ForEach(ResolveShelves<IKeyValueStore>(loadingDockPolicies), (shelf) =>
            {
                shelf.Store(scope, key, data, enforcedPolicies);
            });

            var uuid = Guid.NewGuid();

            var receipt = new Receipt(enforcedPolicies.Any())
            {
                UUID = uuid,
                Key = key,
                Scope = scope,
                // add the policies that were upheld during the store, this is necessary, 
                // because this warehouse might not be able to satisfy all of the policies				
                Policies = enforcedPolicies.Distinct().ToList(),
                SHA256Checksum = CalculateChecksum(data)
            };

            SessionReceipts.Add(receipt);

            TriggerBackgroundSync();

            return receipt;
        }

        public IEnumerable<T> ResolveShelves<T>(IEnumerable<StoragePolicy> loadingDockPolicies)
            where T : IStore
        {
            return Stores.OfType<T>().Where(s => s.CanEnforcePolicies(loadingDockPolicies));
        }

        void ThrowIfNotInitialized()
		{
			if (!IsInitialized)
				throw new InvalidOperationException("Warehouse not initialized.");
		}

		public static string CalculateChecksum(object input)
		{
			// can't calculate checksums for non strings right now
			// TODO: add support for non-strings
			if (input.GetType() != typeof(string))
				return String.Empty;
            
            using (var sha256 = SHA256.Create())
			{
				byte[] data = sha256.ComputeHash(Encoding.UTF8.GetBytes(input as string));
				var sBuilder = new StringBuilder();
				for (int i = 0; i < data.Length; i++)				
					sBuilder.Append(data[i].ToString("x2"));
				
				return sBuilder.ToString();
			}
		}

		public static bool VerifyChecksum(string input, string hash)
		{
			return CalculateChecksum(input).Equals(hash);
		}

		public StorageKeyManifest GetManifest(IStorageScope scope, string key)
		{
            //// right now, we just return the data that was sent when it was created
            //var policies = SessionReceipts.FirstOrDefault(sr => sr.Key == key.Id)?.Policies;

            //// if there aren't any receipts for this, the warehouse has no idea where they're stored. 
            //// TODO: ideally, the warehouse will eventually be able to resolve the receipts from their state
            //if (policies == null)
            //	return new StorageKeyManifest();

            //return new StorageKeyManifest
            //{
            //	// TODO: where do we get the type from? is it passed in? Why should it matter here?
            //	StorageShelvesManifests = ResolveShelves<object>(policies).Where(s => s.CanRetrieve(key)).Select( shelf => shelf.GetManifest(key)).ToList(),
            //	StoragePolicies = policies
            //};	
            return null;
		}

        public T Retrieve<T>(IStorageScope scope, string key)
        {
            ThrowIfNotInitialized();

            if(typeof(T) == typeof(string))
            {
                // TODO: another hack
                throw new InvalidOperationException("Should not store strings with this method. Use the non-generic version for that.");
            }

            // this is a hack until we can figure out which store to pick (different versions of the same file?)
            var objectStore = Stores
                    .OfType<IObjectStore>()
                    .FirstOrDefault(store => store.CanRetrieve(scope, key));

            if (objectStore == null)
            {
                throw new ApplicationException($"Data can't be found: {key} ({typeof(T)}).");
            }

            var val = objectStore.Retrieve<T>(scope, key);

            if (val != null)
                return val;
            else
                return default;
        }

        public string Retrieve(IStorageScope scope, string key)
        {
            ThrowIfNotInitialized();
            // this is a hack until we can figure out which store to pick (different versions of the same file?)
            var keyValStore = Stores
                    .OfType<IKeyValueStore>()
                    .FirstOrDefault(store => store.CanRetrieve(scope, key));

            if (keyValStore == null)
            {
                throw new ApplicationException($"Can't find any KeyValue stores that have the data.");
            }

            var val = keyValStore.Retrieve(scope, key);

            if (val != null)
                return val;
            else
                return default;
        }

        public StorageResponse Handle(StorageRequest request)
        {
            switch (request.Action)
            {
                case StorageAction.Store:
                    return Store(request);
                case StorageAction.Retrieve:
                    return Retrieve(request);
                case StorageAction.Inspect:
                    return Inspect(request);
                case StorageAction.Delete:
                    return Delete(request);
                default:
                    return new StorageResponse(false, "");
            }
        }

        private StorageResponse Delete(StorageRequest request)
        {
            if (request.PayloadType == StoragePayloadType.Blob)
                Store(StorageScope.Global, request.Key, null, request.LoadingDockPolicies);
            else
                Store(StorageScope.Global, request.Key, null, request.LoadingDockPolicies);

            return new StorageResponse();
        }

        private StorageResponse Inspect(StorageRequest request)
        {
            var manifest = GetManifest(StorageScope.Global, request.Key);

            return new StorageResponse
            {
                Manifest = manifest
            };
        }

        private StorageResponse Retrieve(StorageRequest request)
        {
            string value = Retrieve(StorageScope.Global, request.Key);

            return new StorageResponse
            {
                StringData = value
            };
        }

        private StorageResponse Store(StorageRequest request)
        {
            Receipt receipt = null;
            if (request.PayloadType == StoragePayloadType.Blob)
                receipt = Store(StorageScope.Global, request.Key, request.Data, request.LoadingDockPolicies);
            else
                receipt = Store(StorageScope.Global, request.Key, request.StringData, request.LoadingDockPolicies);

            return new StorageResponse
            {
                Receipt = receipt
            };
        }
    }

    public class WarehouseConfig
    {
        public List<WarehouseConnection> Connections { get; set; }
    }

    public class WarehouseConnection
    {
        public WarehouseServerConnectionType Type { get; set; }
        public string ConnectionString { get; set; }
    }

    public enum WarehouseServerConnectionType
    {
        KeyValue,
        Relational,
        Blob
    }

    public class StorageResponse
    {
        public static StorageResponse Stored = new StorageResponse();

        public StorageResponse(bool wasSuccessful = true, string message = null)
        {
            WasSuccessful = wasSuccessful;
            Message = message;
        }

        public bool WasSuccessful { get; }
        public string Message { get; }
        public Receipt Receipt { get; internal set; }
        public byte[] Data { get; set; }
        public string StringData { get; set; } // TODO: is this a necessary hack?!
        public StorageKeyManifest Manifest { get; internal set; }
    }

    public class StorageRequest
    {
        public StoragePayloadType PayloadType { get; set; }
        public StorageAction Action { get; set; }
        public byte[] Data { get; set; }
        public string StringData { get; set; } // TODO: is this a necessary hack?!
        public string Key { get; set; }
        public IEnumerable<StoragePolicy> LoadingDockPolicies { get; set; }
    }

    public enum StorageAction
    {
        Store,
        Retrieve,
        Delete,
        Inspect
    }

    public enum StoragePayloadType
    {
        String,
        Blob
    }
}
