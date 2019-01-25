using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Storage
{
	/// <summary>
	/// Stores a particular type and scope of data
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IShelf<T> : IShelf, IEqualityComparer<IShelf<T>>
	{
		void Store(StorageKey key, T payload, IProducerConsumerCollection<StoragePolicy> enforcedPolicies);
		void Append(StorageKey key, T additionalPayload);		
		T Retrieve(StorageKey key);
	}

	/// <summary>
	/// A shelf is a type of storage sink. So if it's in-memory, AWS S3, or Redis, it represents a method of persisting data.
	/// Warehouses, will organize and decide about the movement of data between shelves
	/// </summary>
	public interface IShelf
	{
		void Initialize(IWarehouse warehouse, IStorageScope scope);

		// Identification aspects
		string Identifier { get; }
		IWarehouse Warehouse { get; }
		IStorageScope Scope { get; }

		// Retrieval Operations
		bool CanRetrieve(StorageKey key);
		ShelfManifest GetManifest(StorageKey key);

		// Discovery Operations
		bool CanEnforcePolicies(IEnumerable<StoragePolicy> loadingDockPolicies);
	}
}
