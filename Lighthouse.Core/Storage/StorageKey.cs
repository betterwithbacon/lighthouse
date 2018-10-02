using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Storage
{
	public sealed class StorageKey : IEqualityComparer<StorageKey>
	{
		public readonly string Id;
		public readonly IStorageScope Scope;
		public readonly Dictionary<string, object> OtherIdentifiers;

		private StorageKey() {
			Id = "";
			Scope = null;
			OtherIdentifiers = null;
		}

		public StorageKey(string uuid, IStorageScope scope, Dictionary<string, object> otherIdentifiers = null)
		{
			Id = uuid;
			Scope = scope;
			OtherIdentifiers = otherIdentifiers;
		}

		public override string ToString()
		{
			return $"[{Scope}]{Id}";
		}

		public bool Equals(StorageKey x, StorageKey y)
		{
			return x.Id == y.Id && x.Scope == y.Scope;
		}

		public int GetHashCode(StorageKey obj)
		{
			return Id.GetHashCode();
		}
	}
}
