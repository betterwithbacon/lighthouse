using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Storage
{
	public class ApplicationScope : IStorageScope
	{	
		public string ScopeName { get; }

		public string Identifier => ScopeName.ToLower().Replace(" ","_");

		public ApplicationScope(string name)
		{
			ScopeName = name;
		}

		public bool Equals(IStorageScope x, IStorageScope y)
		{
			return x.Identifier == y.Identifier;
		}

		public int GetHashCode(IStorageScope obj)
		{
			return obj.Identifier.GetHashCode();
		}
	}

	public sealed class StorageScope : IStorageScope
	{
		public static IStorageScope Global = new StorageScope("global");
		
		public string ScopeName { get; }

		public string Identifier => ScopeName;

		public StorageScope(string name)
		{
			ScopeName = name;
		}

		public bool Equals(IStorageScope x, IStorageScope y)
		{
			throw new NotImplementedException();
		}

		public int GetHashCode(IStorageScope obj)
		{
			return obj.Identifier.GetHashCode();
		}
	}

	public sealed class LocalScope : IStorageScope
	{	
		public string ScopeName { get; }

		public string Identifier => ScopeName;

		public LocalScope()
		{
			// generate a GUID to identify this scope. it's not expected to be persistent
			ScopeName = $"local_{Guid.NewGuid().ToString().Replace("-","")}";
		}

		public bool Equals(IStorageScope x, IStorageScope y)
		{
			throw new NotImplementedException();
		}

		public int GetHashCode(IStorageScope obj)
		{
			return obj.Identifier.GetHashCode();
		}
	}
}
