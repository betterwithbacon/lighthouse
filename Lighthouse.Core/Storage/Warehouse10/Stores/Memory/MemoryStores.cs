using Lighthouse.Core;
using Lighthouse.Core.Storage;
using System;
//using System.Collections.Async;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.Storage.Legacy.Stores.Memory
{
    public class InMemoryObjectStore : IObjectStore
    {        
        readonly ConcurrentDictionary<(IStorageScope, string), object> data = new ConcurrentDictionary<(IStorageScope, string), object>();

        private ILighthouseServiceContainer Container { get; set; }

        public bool CanEnforcePolicies(IEnumerable<StoragePolicy> loadingDockPolicies)
        {
            return loadingDockPolicies == null || loadingDockPolicies.Contains(StoragePolicy.Ephemeral);
        }

        public bool CanRetrieve(IStorageScope scope, string key)
        {
            return data.ContainsKey((scope, key));
        }

        public async Task<IEnumerable<ItemDescriptor>> GetManifests(IStorageScope scope, string key = null)
        {
            var items = new List<ItemDescriptor>();

            foreach(var obj in data.Keys)
            {
                items.Add(
                    new ItemDescriptor
                    {
                        Key = obj.Item2,
                        Scope = obj.Item1
                    });
            }

            return await Task.FromResult(items);
        }

        public void Initialize(ILighthouseServiceContainer container)
        {
            Container = container;
        }

        public T Retrieve<T>(IStorageScope scope, string key)
        {
            var val = data[(scope, key)] ;

            if (val == null)
                return default;

            return (T)val;
        }

        public void Store(IStorageScope scope, string key, object payload, IProducerConsumerCollection<StoragePolicy> enforcedPolicies)
        {
            enforcedPolicies.TryAdd(StoragePolicy.Ephemeral);
            data.AddOrUpdate((scope, key), payload, (s, k) => payload);
        }

        public void Store(IStorageScope scope, string key, object payload)
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryKeyValueStore : IKeyValueStore
    {
        readonly ConcurrentDictionary<(IStorageScope, string), string> data = new ConcurrentDictionary<(IStorageScope, string), string>();

        private ILighthouseServiceContainer Container { get; set; }

        public bool CanEnforcePolicies(IEnumerable<StoragePolicy> loadingDockPolicies)
        {
            return loadingDockPolicies == null || loadingDockPolicies.Contains(StoragePolicy.Ephemeral);
        }

        public bool CanRetrieve(IStorageScope scope, string key)
        {
            return data.ContainsKey((scope, key));
        }

        public async Task<IEnumerable<ItemDescriptor>> GetManifests(IStorageScope scope, string key = null)
        {
            return await Task.Run(() =>
               {
                   var foundDescriptors = new ConcurrentBag<ItemDescriptor>();
                   
                   foreach(var rec in data)
                   {
                       foundDescriptors.Add(new ItemDescriptor
                       {
                           Key = rec.Key.Item2,
                           Scope = rec.Key.Item1
                       });
                   }

                   return foundDescriptors;
               });
        }

        public void Initialize(ILighthouseServiceContainer container)
        {
            Container = container;
        }

        public string Retrieve(IStorageScope scope, string key)
        {
            //return data[(scope, key)];
            if(data.TryGetValue((scope,key), out var val))
            {
                return val;
            }
            else
            {
                return null;
            }
        }

        public void Store(IStorageScope scope, string key, string payload) //, IProducerConsumerCollection<StoragePolicy> enforcedPolicies)
        {
            //enforcedPolicies.TryAdd(StoragePolicy.Ephemeral);
            data.AddOrUpdate((scope, key), payload, (s, k) => payload);
        }
    }
}
