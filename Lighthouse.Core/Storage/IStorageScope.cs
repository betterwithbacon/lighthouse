using System.Collections.Generic;

namespace Lighthouse.Core.Storage
{
	public interface IStorageScope : IEqualityComparer<IStorageScope>
	{
		string ScopeName { get; }

		string Identifier { get; }
	}

    public class StorageOperation
    {

    }
}