using Lighthouse.Core;
using Lighthouse.Storage.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
			//Container.AddScheduledAction(
			//	new Schedule(ScheduleFrequency.Minutely, 10), 
			//	(time) => Sync(time) );

			// c:\folder1 --> FileSyncFolderStatus
			//	FileSyncFolderStatus 
			//		file1 --> 1/1/2019
			//		file2 --> 1/2/2019
			// c:\folder2 --> FileSyncFolderStatus
			FolderStatus = new WarehouseDictionary<string, FileSyncFolderStatus>(Container.Warehouse, this , $"{nameof(FileSyncApp)}.{nameof(FolderStatus)}");
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
}
