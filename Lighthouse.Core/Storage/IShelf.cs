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
		// Storage operations
		void Append(StorageKey key, IEnumerable<T> additionalPayload);
		void Store(StorageKey key, IEnumerable<T> payload, IProducerConsumerCollection<StoragePolicy> enforcedPolicies);
		IEnumerable<T> Retrieve(StorageKey key);
	}

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
