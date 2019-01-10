using Lighthouse.Core;
using Lighthouse.Core.Scheduling;
using Lighthouse.Core.Storage;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Lighthouse.Server.Apps.FileSync
{
	public class FileSyncFolderStatus
	{
		public Dictionary<string, DateTime> FileUpdateDate { get; set; } = new Dictionary<string, DateTime>();
	}

	public class FileSyncApp : LighthouseServiceBase
	{
		private static int _lockFlag = 0; // 0 - free
		private static readonly ConcurrentDictionary<string, bool> activeFileSyncs = new ConcurrentDictionary<string, bool>();
		protected readonly WarehouseDictionary<string, FileSyncFolderStatus> FolderStatus;

		public FileSyncApp()
		{
			// sync every 10 minutes 
			// TODO: make this time configurable
			Container.AddScheduledAction(
				new Schedule(ScheduleFrequency.Minutely, 10), 
				(time) => Sync(time) );

			// c:\folder1 --> FileSyncFolderStatus
			//	FileSyncFolderStatus 
			//		file1 --> 1/1/2019
			//		file2 --> 1/2/2019
			// c:\folder2 --> FileSyncFolderStatus
			FolderStatus = new WarehouseDictionary<string, FileSyncFolderStatus>(Container.Warehouse, $"{nameof(FileSyncApp)}.{nameof(FolderStatus)}");
		}
		
		public void Sync(DateTime time)
		{
			// every 10 minute this thread will wake up and look for file changes, it will then put that work on background task to process the files
			if (Interlocked.CompareExchange(ref _lockFlag, 1, 0) == 0)
			{
				// track progress in the key/value store
				
				
				Interlocked.Decrement(ref _lockFlag);
			}
		}
	}

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
		private readonly Dictionary<TKey, TValue> internalDictionary = new Dictionary<TKey, TValue>();

		public WarehouseDictionary(IWarehouse warehouse, string dictionaryName)
		{
			Warehouse = warehouse;
			this.dictionaryName = dictionaryName;
		}

		public TValue this[TKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public ICollection<TKey> Keys => throw new NotImplementedException();

		public ICollection<TValue> Values => throw new NotImplementedException();

		public int Count => throw new NotImplementedException();

		public bool IsReadOnly => throw new NotImplementedException();

		public void Add(TKey key, TValue value)
		{
			throw new NotImplementedException();
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public bool ContainsKey(TKey key)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public bool Remove(TKey key)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			throw new NotImplementedException();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
