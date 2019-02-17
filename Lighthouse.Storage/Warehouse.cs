
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lighthouse.Core;
using Lighthouse.Core.Scheduling;
using Lighthouse.Core.Storage;
using Lighthouse.Storage.Memory;

namespace Lighthouse.Storage
{
	public class Warehouse : LighthouseServiceBase, IWarehouse
	{
		// TODO: this is atemp solution to this problem of discoverying available shelf types
		// ideally, this will be discovered by reflection
		public static ConcurrentBag<Type> AvailableShelfTypes;
		public readonly List<Receipt> SessionReceipts = new List<Receipt>();
		readonly ConcurrentBag<IStore> Shelves = new ConcurrentBag<IStore>();		
		private readonly ConcurrentBag<Warehouse> RemoteWarehouses = new ConcurrentBag<Warehouse>();

		protected override bool IsInitialized => Shelves.Count > 0;

		static Warehouse()
		{
			AvailableShelfTypes = new ConcurrentBag<Type>();
		}

		public static void RegisterShelfType(Type shelfType)
		{
			if (shelfType.IsAssignableFrom(typeof(IStore)) && shelfType.IsClass)
				throw new ArgumentException("shelf type MUST be implement IShelf and be concrete.");

			if (!AvailableShelfTypes.Contains(shelfType))
				AvailableShelfTypes.Add(shelfType);
		}

		protected override void OnInit()
		{
			// Discover shelf types
			foreach (var shelf in DiscoverShelves())
			{
				Shelves.Add(shelf);

				// create all the shelves in the global scope
				shelf.Initialize(this);
			}

			//foreach (var shelfType in shelvesToUse)
			//{
			//	if (Activator.CreateInstance(shelfType) is IShelf shelf)
			//	{
			//		Shelves.Add(shelf);

			//		// create all the shelves in the global scope
			//		shelf.Initialize(this, StorageScope.Global);
			//	}
			//}
		}

		protected override void OnAfterStart()
		{
			// schedule server maintainence to be done each hour
			Container.AddScheduledAction(schedule: new Schedule(ScheduleFrequency.Hourly) , taskToPerform: (time) => { PerformStorageMaintenance(time); });

			// populate the remote warehouses			
			LoadRemoteWarehouses().RunSynchronously();
		}

		public IEnumerable<IStore> DiscoverShelves()
		{
			yield return new MemoryShelf();


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

		private async Task LoadRemoteWarehouses()
		{
			// the container is how remote lighthouse resources are found
			if (Container != null)
			{
				Container.Log(Lighthouse.Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, this, "Loading remote warehouses.");

				// the Lighthouse context should know about the other services that are running
				foreach (var remoteWarehouse in Container.FindServices<Warehouse>())
				{
					// skip THIS service.
					if (remoteWarehouse.Id == this.Id)
						continue;

					RemoteWarehouses.Add(remoteWarehouse);
					Container.Log(Lighthouse.Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, this, $"Container local warehouse {remoteWarehouse} was added.");
				}

				// this is where an network discovery will occur. to reach other points, not local to this lighthouse runtime.
				// currently, this isn't implemented, but ideally
				foreach (var remoteWarehouseProxy in await Container.FindRemoteServices<Warehouse>())
				{
					//// skip THIS service.
					//if (remoteWarehouseProxy.Service.Id == this.Id)
					//	continue;

					//RemoteWarehouses.Add(remoteWarehouse);
					//LighthouseContainer.Log(Lighthouse.Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, this, $"Remote warehouse {remoteWarehouse} was added.");
				}
			}
		}
		
		private void PerformStorageMaintenance(DateTime date)
		{
		}

        //public Receipt Store<T>(StorageKey key, T data, IEnumerable<StoragePolicy> loadingDockPolicies)
        public Receipt Store(IStorageScope scope, string key, object data, IEnumerable<StoragePolicy> loadingDockPolicies = null)
        {
			ThrowIfNotInitialized();

			var uuid = Guid.NewGuid();

			ConcurrentBag<StoragePolicy> enforcedPolicies = new ConcurrentBag<StoragePolicy>();

			// resolve the appropriate store, based on the policy
			Parallel.ForEach(ResolveShelves<T>(loadingDockPolicies), (shelf) =>
			{
				shelf.Store(key,data, enforcedPolicies);
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
            throw new NotImplementedException();
        }

        public T Retrieve<T>(StorageKey key)			
		{
			ThrowIfNotInitialized();
			var foundShelf = Shelves
					.OfType<IStore<T>>()
					.FirstOrDefault(shelf => shelf.CanRetrieve(key));

			if (foundShelf == null)
			{
				throw new ApplicationException($"Can't store this type of data: {typeof(T)}.");
			}

			var val = foundShelf.Retrieve(key);

			if (val != null)
				return val;
			else
				return default;
		}

		public IEnumerable<IStore<T>> ResolveShelves<T>(IEnumerable<StoragePolicy> loadingDockPolicies)
		{
			return Shelves.OfType<IStore<T>>().Where(s => s.CanEnforcePolicies(loadingDockPolicies));
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
            throw new NotImplementedException();
        }

        public string Retrieve(IStorageScope scope, string key)
        {
            throw new NotImplementedException();
        }
    }
}
