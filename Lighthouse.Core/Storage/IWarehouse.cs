using Lighthouse.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lighthouse.Core.Storage
{
    // A simple storage abstraction. It sits directly on top of a persistent (or not) storage mechanism...
    // ... and provides a common interface for writing to and reading from. 
    // Writing to a single warehouse should also permit replication based on storage type
	public interface IWarehouse
	{
        ILighthouseServiceContainer Container { get; }

		Receipt Store(IStorageScope scope, string key, object data, IEnumerable<StoragePolicy> loadingDockPolicies = null);

        Receipt Store(IStorageScope scope, string key, string data, IEnumerable<StoragePolicy> loadingDockPolicies = null);

		T Retrieve<T>(IStorageScope scope, string key);

        string Retrieve(IStorageScope scope, string key);

        IEnumerable<ItemDescriptor> GetManifest(IStorageScope scope, string key = null);

        Task<IEnumerable<StorageOperation>> PerformStorageMaintenance(DateTime time);
    }
}
