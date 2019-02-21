using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Storage
{
    /// <summary>
    /// A specialized store of objects.
    /// </summary>
    public interface IObjectStore : IStore
    {
        // Storage Operations
        void Store(IStorageScope scope, string key, object payload, IProducerConsumerCollection<StoragePolicy> enforcedPolicies);
        T Retrieve<T>(IStorageScope scope, string key);
    }

    /// <summary>
    /// An optimized key/value store, where they values are always strings. However, the retrieval should be nominally faster
    /// Also if a consumer wants to store serialized object graphs in one of these, that's an option too I guess.
    /// </summary>
    public interface IKeyValueStore : IStore
    {
        // Storage Operations
        void Store(IStorageScope scope, string key, string payload, IProducerConsumerCollection<StoragePolicy> enforcedPolicies);
        string Retrieve(IStorageScope scope, string key);
    }

    /// <summary>
    /// A shelf is a type of storage sink. So if it's in-memory, AWS S3, or Redis, it represents a method of persisting data.
    /// Warehouses, will organize and decide about the movement of data between shelves
    /// </summary>
    public interface IStore
	{
		void Initialize(ILighthouseServiceContainer container);

		// Retrieval Operations
		bool CanRetrieve(IStorageScope scope, string key);

        // Metadata operations 
        ShelfManifest GetManifest(IStorageScope scope, string key); // <-- hmmmmm??

        // Discovery Operations
        bool CanEnforcePolicies(IEnumerable<StoragePolicy> loadingDockPolicies);
	}
}
