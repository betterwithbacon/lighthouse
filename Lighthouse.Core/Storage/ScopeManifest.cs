using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lighthouse.Core.Storage
{
    public class ScopeManifest
    {
        private IEnumerable<KeyValuePair<(IStorageScope, string), string>> enumerable;

        public ScopeManifest(IEnumerable<KeyValuePair<(IStorageScope, string), string>> enumerable)
        {
            this.Items = enumerable.Select(v => v.Key);
        }

        private IEnumerable<(IStorageScope, string)> Items { get; }

        public Task<IEnumerable<ItemDescriptor>> GetItems()
        {
            throw new NotImplementedException();
        }
    }

    public class ItemDescriptor
    {
        public IStorageScope Scope { get; set; }
        public string Key { get; set; }
    }
}
