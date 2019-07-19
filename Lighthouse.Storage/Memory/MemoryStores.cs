using Lighthouse.Core;
using Lighthouse.Core.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Storage.Memory
{
    //public class KeyValueMemoryShelf : IStore<IDictionary<string, string>>
    //{
    //	public static readonly IList<StoragePolicy> SupportedPolicies = new[] { StoragePolicy.Ephemeral };
    //	public string Identifier => Guid.NewGuid().ToString();
    //	public IWarehouse Warehouse { get; private set; }
    //	public IStorageScope Scope { get; private set; }
    //	private static readonly ConcurrentDictionary<StorageKey, Dictionary<string,string>> Records = new ConcurrentDictionary<StorageKey, Dictionary<string, string>>();

    //	public void Append(StorageKey key, IDictionary<string, string> additionalPayload)
    //	{
    //		throw new NotImplementedException();
    //	}

    //	public bool CanEnforcePolicies(IEnumerable<StoragePolicy> loadingDockPolicies)
    //	{
    //		throw new NotImplementedException();
    //	}

    //	public bool CanRetrieve(StorageKey key)
    //	{
    //		return Records.Keys.Contains(new MemoryShelfKey(key.Scope, key.Id));
    //	}

    //	public bool Equals(IStore<IDictionary<string, string>> x, IStore<IDictionary<string, string>> y)
    //	{
    //		// TODO: lol
    //		return false;
    //	}

    //	public int GetHashCode(IStore<IDictionary<string, string>> obj)
    //	{
    //		throw new NotImplementedException();
    //	}

    //	public ShelfManifest GetManifest(StorageKey key)
    //	{
    //		throw new NotImplementedException();
    //	}

    //	public void Initialize(IWarehouse warehouse, IStorageScope scope)
    //	{
    //		throw new NotImplementedException();
    //	}

    //	public IDictionary<string, string> Retrieve(StorageKey key)
    //	{
    //		throw new NotImplementedException();
    //	}

    //	public void Store(StorageKey key, IDictionary<string, string> payload, IProducerConsumerCollection<StoragePolicy> enforcedPolicies)
    //	{
    //		throw new NotImplementedException();
    //	}
    //}

    //public class MemoryShelf 
    //	: IObjectStore, IKeyValueStore
    //   {
    //	private static readonly ConcurrentDictionary<MemoryShelfKey, string> Records = new ConcurrentDictionary<MemoryShelfKey, string>();
    //	public static readonly IList<StoragePolicy> SupportedPolicies = new[] {  StoragePolicy.Ephemeral };
    //	public string Identifier => Guid.NewGuid().ToString();
    //	public IWarehouse Warehouse { get; private set; }
    //	public IStorageScope Scope { get; private set; }

    //	//public bool CanRetrieve(StorageKey key)
    //	//{
    //	//	return Records.Keys.Contains(new MemoryShelfKey(key.Scope, key.Id));
    //	//}

    //	#region IShelf<string>
    //	public string Retrieve(StorageKey key)
    //	{
    //		return Records.GetValueOrDefault(new MemoryShelfKey(key.Scope, key.Id), "");
    //	}

    //	public void Append(StorageKey key, string additionalPayload)
    //	{
    //		Records.AddOrUpdate(new MemoryShelfKey(key.Scope, key.Id), additionalPayload, (k, a) => a += additionalPayload);
    //	}

    //	public void Store(StorageKey key, string payload, IProducerConsumerCollection<StoragePolicy> enforcedPolicies)
    //	{
    //		Records.AddOrUpdate(new MemoryShelfKey(key.Scope, key.Id), payload,(k,a) => payload);
    //		foreach(var pol in SupportedPolicies)
    //			enforcedPolicies.TryAdd(pol);
    //	}

    //	public bool CanEnforcePolicies(IEnumerable<StoragePolicy> loadingDockPolicies)
    //	{
    //		return SupportedPolicies.Any(
    //			sp => loadingDockPolicies.Any(
    //				ldp => ldp.IsEquivalentTo(sp)
    //			)
    //		);
    //	}

    //	public bool Equals(IStore<string> x, IStore<string> y)
    //	{
    //		return x.Identifier == y.Identifier;
    //	}

    //	public int GetHashCode(IStore<string> obj)
    //	{
    //		// TODO: this is TERRIBLE, because it means that another shelf with the same ID, could be confused for this one
    //		return obj.Identifier.GetHashCode();
    //	}
    //	#endregion

    //	#region Helpers
    //	public ShelfManifest GetManifest(StorageKey key)
    //	{
    //		if (CanRetrieve(key))
    //		{
    //			// the only way to get metadata from a memory shelf is to actually just retrieve it. 
    //			// TODO: store metadata separately for items as stord
    //			return new ShelfManifest(SupportedPolicies, CalculateSize(Retrieve(key)));
    //		}

    //		return null;
    //	}

    //	private long CalculateSize(string record)
    //	{
    //		return Encoding.Unicode.GetByteCount(record);			
    //	}

    //	public void Initialize(IWarehouse warehouse, IStorageScope scope)
    //	{
    //		Warehouse = warehouse;
    //		Scope = scope;

    //		// there's no other work for a memshelf
    //	}
    //	#endregion

    //	#region MemoryShelfKey
    //	struct MemoryShelfKey
    //	{
    //		public string ScopeIdentifier { get; }
    //		public string Key { get; }

    //		public MemoryShelfKey(IStorageScope scope, string key)
    //		{
    //			ScopeIdentifier = scope.Identifier;
    //			Key = key;
    //		}

    //		public override bool Equals(object obj)
    //		{
    //			if (obj is MemoryShelfKey msk)
    //				return msk.GetHashCode() == this.GetHashCode();
    //			else
    //				return false;
    //		}

    //		public override int GetHashCode()
    //		{
    //			return HashCode.Combine<string, string>(ScopeIdentifier, Key);
    //		}
    //	}
    //	#endregion
    //}

    public class InMemoryObjectStore : IObjectStore
    {        
        readonly ConcurrentDictionary<(IStorageScope, string), object> data = new ConcurrentDictionary<(IStorageScope, string), object>();

        private ILighthouseServiceContainer Container { get; set; }

        public bool CanEnforcePolicies(IEnumerable<StoragePolicy> loadingDockPolicies)
        {
            return loadingDockPolicies.Contains(StoragePolicy.Ephemeral);
        }

        public bool CanRetrieve(IStorageScope scope, string key)
        {
            return data.ContainsKey((scope, key));
        }

        public Task<StoreManifest> GetManifest(IStorageScope scope, string key)
        {
            // TODO: actually make this do something interesting
            return Task.FromResult(new StoreManifest(new[] { StoragePolicy.Ephemeral }, -1));
        }

        public Task<ScopeManifest> GetManifest(IStorageScope scope)
        {
            throw new NotImplementedException();
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
    }

    public class InMemoryKeyValueStore : IKeyValueStore
    {
        readonly ConcurrentDictionary<(IStorageScope, string), string> data = new ConcurrentDictionary<(IStorageScope, string), string>();

        private ILighthouseServiceContainer Container { get; set; }

        public bool CanEnforcePolicies(IEnumerable<StoragePolicy> loadingDockPolicies)
        {
            return loadingDockPolicies.Contains(StoragePolicy.Ephemeral);
        }

        public bool CanRetrieve(IStorageScope scope, string key)
        {
            return data.ContainsKey((scope, key));
        }

        public Task<StoreManifest> GetManifest(IStorageScope scope, string key)
        {
            return Task.FromResult(new StoreManifest(new[] { StoragePolicy.Ephemeral }, key.Length));
        }

        public Task<ScopeManifest> GetManifest(IStorageScope scope)
        {
            throw new NotImplementedException();
        }

        public void Initialize(ILighthouseServiceContainer container)
        {
            Container = container;
        }

        public string Retrieve(IStorageScope scope, string key)
        {
            return data[(scope, key)];
        }

        public void Store(IStorageScope scope, string key, string payload, IProducerConsumerCollection<StoragePolicy> enforcedPolicies)
        {
            enforcedPolicies.TryAdd(StoragePolicy.Ephemeral);
            data.AddOrUpdate((scope, key), payload, (s, k) => payload);
        }
    }
}
