using System.Collections.Generic;

namespace Lighthouse.Core.Storage
{
	public class StorageKeyManifest
	{
		public IList<StoragePolicy> StoragePolicies { get; set; }
		public IEnumerable<ShelfManifest> StorageShelvesManifests { get; set; }
	}
}