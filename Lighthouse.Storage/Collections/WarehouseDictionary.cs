using Lighthouse.Core.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lighthouse.Storage.Collections
{
	/// <summary>
	///  A warehouse dictionary is a key/value store that is backed by a Warehouse.
	///  So ideally it should be possible for a user to use this dictionary, and have the state seameledssly reconstitute itself later	
	///  The syncing is non-deterministic
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	public class WarehouseDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private readonly IWarehouse Warehouse;
		private readonly string dictionaryName;
		// TODO: do we really want to expose the underlying warehouse infra. 
		// I can see it from both perspectives TBH
		public StorageKey StorageKey { get; private set; }
		private readonly Dictionary<TKey, TValue> internalDictionary = new Dictionary<TKey, TValue>();

		public WarehouseDictionary(IWarehouse warehouse, IStorageScope storageScope, string dictionaryName)
		{
			Warehouse = warehouse;
			this.dictionaryName = dictionaryName;
			StorageKey = new StorageKey(dictionaryName, storageScope);

			// retrieve the current values from the warehouse
			internalDictionary = new Dictionary<TKey, TValue>();

			// copy all of the items over
			var existingDictionary = Warehouse.Retrieve<Dictionary<TKey, TValue>>(StorageKey);

			if (existingDictionary != null)
				foreach (var keyVal in existingDictionary)
				{
					internalDictionary.Add(keyVal.Key, keyVal.Value);
				}

			// the two dictionaries are now disconnected, anytime a change is made it should be made to both
		}

		public TValue this[TKey key]
		{
			get
			{
				// always pull from local cache
				return internalDictionary[key];
			}
			set
			{
				// always push to local cache first
				internalDictionary[key] = value;

				// then push to warehouse
				Warehouse.Append(StorageKey, KeyValuePair.Create(key, value));
			}
		}

		public ICollection<TKey> Keys => internalDictionary.Keys;

		public ICollection<TValue> Values => internalDictionary.Values;

		public int Count => internalDictionary.Count;

		// TODO: should we actually be clever about acknowloding when this dictionary might actually be "readonly"
		public bool IsReadOnly => false;

		public void Add(TKey key, TValue value)
		{
			Add(new KeyValuePair<TKey,TValue>(key, value));
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			internalDictionary.Add(item.Key, item.Value);

			// TODO: do this later in a task
			// do warehouse stuff, in a task
			//Warehouse.Container.Do((container) 
			//=> 
			Warehouse.Append(StorageKey, KeyValuePair.Create(item.Key, item.Value)); //);
		}

		public void Clear()
		{
			internalDictionary.Clear();

			// do warehouse stuff, in a task
			//Warehouse.Container.Do((container) =>
			//	{
					// figure out what the old policies were
					var metadata = Warehouse.GetManifest(StorageKey);

					// overwrite what was there
					Warehouse.Store<IDictionary<TKey, TValue>>(StorageKey, internalDictionary, metadata.StoragePolicies);
			//	}
			//);
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>)internalDictionary).Contains(item);			
		}

		public bool ContainsKey(TKey key)
		{
			return internalDictionary.ContainsKey(key);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)internalDictionary).CopyTo(array, arrayIndex);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return internalDictionary.GetEnumerator();
		}

		public bool Remove(TKey key)
		{
			var val = internalDictionary.Remove(key);
			Warehouse.Container.Do((container) =>
				{
					// figure out what the old policies were
					var metadata = Warehouse.GetManifest(StorageKey);
					// overwrite what was there (I don't like this, but we don't support fine-grained actions
					Warehouse.Store(StorageKey, internalDictionary, metadata.StoragePolicies);
				}
			);

			return val;
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			var val = ((ICollection<KeyValuePair<TKey, TValue>>)internalDictionary).Remove(item);
			Warehouse.Container.Do((container) =>
			{
				// figure out what the old policies were
				var metadata = Warehouse.GetManifest(StorageKey);
				// overwrite what was there (I don't like this, but we don't support fine-grained actions
				Warehouse.Store(StorageKey, internalDictionary, metadata.StoragePolicies);
			});
			return val;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return internalDictionary.TryGetValue(key, out value);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return internalDictionary.GetEnumerator();
		}
	}
}
