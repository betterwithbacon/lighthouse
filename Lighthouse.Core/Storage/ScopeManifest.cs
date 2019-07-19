using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lighthouse.Core.Storage
{
    //public class ScopeManifest
    //{
    //    public ScopeManifest(IEnumerable<KeyValuePair<(IStorageScope, string), string>> enumerable = null)
    //    {
    //        this.Items = (enumerable?.Select(v => v.Key) ?? Enumerable.Empty<(IStorageScope, string)>()).Select(i => new ItemDescriptor {
    //            Scope = i.Item1,
    //            Key = i.Item2
    //        });
    //    }

    //    private IEnumerable<ItemDescriptor> Items { get; }

    //    public async Task<IEnumerable<ItemDescriptor>> GetItems()
    //    {
    //        return await Task.Fro//}mResult(Items);
    //    }
    

    public class ItemDescriptor
    {
        public IStorageScope Scope { get; set; }
        public string Key { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public DateTime LastUpdated { get; set; }
        public IList<StoragePolicy> StoragePolicies { get; set; }
    }
}
