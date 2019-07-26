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

		/// <summary>
		/// Stores the provided data, with the relevant key
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <param name="loadingDockPolicies"></param>
		/// <returns></returns>
		Receipt Store(IStorageScope scope, string key, object data, IEnumerable<StoragePolicy> loadingDockPolicies = null, bool syncChangesToOtherWarehouses = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="loadingDockPolicies"></param>
        /// <returns></returns>
        Receipt Store(IStorageScope scope, string key, string data, IEnumerable<StoragePolicy> loadingDockPolicies = null, bool syncChangesToOtherWarehouses = true);
        
		/// <summary>
		/// Returns the Data for a given key and scope
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		T Retrieve<T>(IStorageScope scope, string key);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scope"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string Retrieve(IStorageScope scope, string key);

        /// <summary>
        /// Returns all of the available metadata for a given StorageKey.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        IEnumerable<ItemDescriptor> GetManifest(IStorageScope scope, string key = null);

        Task<IEnumerable<StorageOperation>> PerformStorageMaintenance(DateTime time);
    }
}
