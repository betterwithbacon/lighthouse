using Lighthouse.Core;
using Lighthouse.Core.IO;
using Lighthouse.Core.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.CORE.Storage.Stores.Disk
{
	public class LocalDiskStore : IKeyValueStore, IObjectStore
	{
		private readonly Dictionary<(IStorageScope,string), string> FileNames = new Dictionary<(IStorageScope, string), string>();

		public static readonly List<StoragePolicy> SupportedPolicies = new List<StoragePolicy> { StoragePolicy.Persistent };
		public string Identifier => Guid.NewGuid().ToString();
		public IWarehouse Warehouse { get; private set; }
		public IStorageScope Scope { get; private set; }
		
		public IFileSystemProvider FileSystemProvider { get; private set; }
        public ILighthouseServiceContainer Container { get; private set; }

        public bool CanEnforcePolicies(IEnumerable<StoragePolicy> loadingDockPolicies)
		{
			return loadingDockPolicies.Intersect(SupportedPolicies).Any();
		}

		public bool CanRetrieve(IStorageScope scope, string key)
		{
			var file = GetFilePath(scope, key);
			return FileSystemProvider.FileExists(file);
		}
        
		//public async Task<StoreItemManifest> GetManifest(IStorageScope scope, string key)
		//{
		//	// we can't support returning this data for real yet. It'd be good to pull this data from the file system			
		//	var manifest = new StoreItemManifest(new[] { StoragePolicy.Persistent }, -1);

		//	// TODO: pull some metadata from File system
		//	return await Task.FromResult(manifest);
		//}

		public void Initialize(IWarehouse warehouse, IStorageScope scope)
		{
			Warehouse = warehouse;
			Scope = scope;

			// TODO: if there are 3 file system providers, who do you know which one to use. 
			FileSystemProvider = Warehouse.Container.GetFileSystemProviders().FirstOrDefault();
			if (FileSystemProvider == null)
				throw new ApplicationException("No filesystem could be located.");
		}

        public T Retrieve<T>(IStorageScope scope, string key)
        {
            var stringRep = Retrieve(scope, key);

            if (string.IsNullOrEmpty(stringRep))
                return default;

            return JsonConvert.DeserializeObject<T>(stringRep);
        }

        public string Retrieve(IStorageScope scope, string key)
		{
			// TODO: don't do this asynchronously YET. I'm delaying converting everything to async, but not yet.
			var rawData = FileSystemProvider.ReadStringFromFileSystem(GetFilePath(scope, key));

			if (rawData == null || rawData.Length == 0)
			{
				// no data, just return an empty file
				// TODO: should this be an exception? if there isn't any data. Or is it possible an empty file was created
				return "";
			}
			else
			{
				return rawData;
			}
		}

		public void Store(IStorageScope scope, string key, string payload, IProducerConsumerCollection<StoragePolicy> enforcedPolicies)
		{
			SupportedPolicies.ForEach((ldp) => enforcedPolicies.TryAdd(ldp));

			FileSystemProvider.WriteStringToFileSystem($"\\{scope.Identifier}\\{key}", payload);
		}

        public void Store(IStorageScope scope, string key, object payload, IProducerConsumerCollection<StoragePolicy> enforcedPolicies)
        {
            throw new NotImplementedException();
        }

        string GetFilePath(IStorageScope scope, string key)
		{
			return $"\\{scope.Identifier}\\{key}";
		}

        public void Initialize(IWarehouse warehouse)
        {
            Container = warehouse?.Container;
        }

        public void Initialize(ILighthouseServiceContainer container)
        {
            throw new NotImplementedException();
        }

        public void Store(IStorageScope scope, string key, string payload)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ItemDescriptor>> GetManifests(IStorageScope scope, string key = null)
        {
            throw new NotImplementedException();
        }

        public void Store(IStorageScope scope, string key, object payload)
        {
            throw new NotImplementedException();
        }
    }
}
