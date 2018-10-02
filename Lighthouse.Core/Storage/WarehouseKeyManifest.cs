using System.Collections.Generic;

namespace Lighthouse.Core.Storage
{
	public class WarehouseKeyManifest
	{
		public IList<StoragePolicy> StoragePolicies { get; internal set; }
		public IEnumerable<ShelfManifest> StorageShelvesManifests { get; internal set; }
	}
}