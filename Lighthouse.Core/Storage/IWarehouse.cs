using Lighthouse.Core;
using System;
using System.Collections.Generic;

namespace Lighthouse.Core.Storage
{
	public interface IWarehouse : ILighthouseComponent // : IDictionary<StorageKey, TData><-- one day!
	{
		/// <summary>
		/// Places the warehouse in a state where it can store and retrieve data
		/// </summary>
		void Initialize(ILighthouseServiceContainer lighthouseContainer, params Type[] shelfTypes);

		/// <summary>
		/// Stores the provided data, with the relevant key
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <param name="loadingDockPolicies"></param>
		/// <returns></returns>
		Receipt Store<T>(StorageKey key, IEnumerable<T> data, IEnumerable<StoragePolicy> loadingDockPolicies);

		/// <summary>
		/// Appends data to the existing data 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <param name="loadingDockPolicies"></param>
		void Append<T>(StorageKey key, IEnumerable<T> data, IEnumerable<StoragePolicy> loadingDockPolicies);

		/// <summary>
		/// Returns the Data for a given key and scope
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		IEnumerable<T> Retrieve<T>(StorageKey key);

		/// <summary>
		/// Returns all of the available metadata for a given StorageKey.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		StorageKeyManifest GetManifest(StorageKey key);
	}
}
