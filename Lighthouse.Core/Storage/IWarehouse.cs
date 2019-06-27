using Lighthouse.Core;
using System;
using System.Collections.Generic;

namespace Lighthouse.Core.Storage
{
	public interface IWarehouse // : IDictionary<StorageKey, TData><-- one day!
	{
        ///// <summary>
        ///// Places the warehouse in a state where it can store and retrieve data
        ///// </summary>
        //void Initialize(ILighthouseServiceContainer lighthouseContainer, params Type[] shelfTypes);

        ILighthouseServiceContainer Container { get; }

		/// <summary>
		/// Stores the provided data, with the relevant key
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <param name="loadingDockPolicies"></param>
		/// <returns></returns>
		Receipt Store(IStorageScope scope, string key, object data, IEnumerable<StoragePolicy> loadingDockPolicies = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="loadingDockPolicies"></param>
        /// <returns></returns>
        Receipt Store(IStorageScope scope, string key, string data, IEnumerable<StoragePolicy> loadingDockPolicies = null);


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
        StorageKeyManifest GetManifest(IStorageScope scope, string key);
	}
}
