using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lighthouse.Core.Storage
{
    public class ItemDescriptor : IEqualityComparer<ItemDescriptor>
    {
        public IStorageScope Scope { get; set; }
        public string Key { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public DateTime LastUpdated { get; set; }
        public IList<StoragePolicy> StoragePolicies { get; set; }

        public bool Equals(ItemDescriptor x, ItemDescriptor y) => x.Scope == y.Scope && x.Key == y.Key;

        public int GetHashCode(ItemDescriptor obj) => HashCode.Combine(obj.Scope, obj.Key);
    }
}
