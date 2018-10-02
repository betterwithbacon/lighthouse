using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Storage.Memory
{
	public class MemoryShelf : IShelf<string>
	{
		private static readonly ConcurrentDictionary<MemoryShelfKey, List<string>> Records = new ConcurrentDictionary<MemoryShelfKey, List<string>>();
		public static readonly IList<LoadingDockPolicy> SupportedPolicies = new[] {  LoadingDockPolicy.Ephemeral };
		public string Identifier => Guid.NewGuid().ToString();
		public IWarehouse Warehouse { get; private set; }
		public IStorageScope Scope { get; private set; }

		public bool CanRetrieve(WarehouseKey key)
		{
			return Records.Keys.Contains(new MemoryShelfKey(key.Scope, key.Id));
		}

		public IEnumerable<string> Retrieve(WarehouseKey key)
		{
			return Records.GetValueOrDefault(new MemoryShelfKey(key.Scope, key.Id), new List<string>());
		}

		public void Append(WarehouseKey key, IEnumerable<string> additionalPayload)
		{
			Records.AddOrUpdate(new MemoryShelfKey(key.Scope, key.Id), additionalPayload.ToList(), (k, a) => a.Concat(additionalPayload).ToList());
		}

		public void Store(WarehouseKey key, IEnumerable<string> payload, IProducerConsumerCollection<LoadingDockPolicy> enforcedPolicies)
		{
			Records.AddOrUpdate(new MemoryShelfKey(key.Scope, key.Id), payload.ToList(),(k,a) => payload.ToList());
			foreach(var pol in SupportedPolicies)
				enforcedPolicies.TryAdd(pol);
		}

		public bool CanEnforcePolicies(IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			return SupportedPolicies.Any(
				sp => loadingDockPolicies.Any(
					ldp => ldp.IsEquivalentTo(sp)
				)
			);
		}

		public bool Equals(IShelf<string> x, IShelf<string> y)
		{
			return x.Identifier == y.Identifier;
		}

		public int GetHashCode(IShelf<string> obj)
		{
			// TODO: this is TERRIBLE, because it means that another shelf with the same ID, could be confused for this one
			return obj.Identifier.GetHashCode();
		}

		public ShelfManifest GetManifest(WarehouseKey key)
		{
			if(CanRetrieve(key))
			{
				// the only way to get metadata from a memory shelf is to actually just retrieve it. 
				// TODO: store metadata separately for items as stord
				return new ShelfManifest(SupportedPolicies, CalculateSize(Retrieve(key)));
			}

			return null;
		}

		#region Helpers
		private long CalculateSize(IEnumerable<string> records)
		{
			var size = 0L;			
			//long size = records.Aggregate(0, 
			//	(totalRecordSize, record) => totalRecordSize += Encoding.Unicode.GetByteCount(record)
			//);

			return size;
		}
		#endregion

		public void Initialize(IWarehouse warehouse, IStorageScope scope)
		{
			Warehouse = warehouse;
			Scope = scope;
			
			// there's no other work for a memshelf
		}

		#region MemoryShelfKey
		struct MemoryShelfKey
		{
			public string ScopeIdentifier { get; }
			public string Key { get; }

			public MemoryShelfKey(IStorageScope scope, string key)
			{
				ScopeIdentifier = scope.Identifier;
				Key = key;
			}

			public override bool Equals(object obj)
			{
				if (obj is MemoryShelfKey msk)
					return msk.GetHashCode() == this.GetHashCode();
				else
					return false;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine<string, string>(ScopeIdentifier, Key);
			}
		}
		#endregion
	}
}
