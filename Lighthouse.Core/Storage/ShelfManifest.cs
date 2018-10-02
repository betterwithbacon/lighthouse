using System.Collections.Generic;

namespace Lighthouse.Core.Storage
{
	/// <summary>
	/// Represents a simple manifest of a shelf. The manifest will be limited by Scope and, optionally, key
	/// </summary>
	public sealed class ShelfManifest
	{
		/// <summary>
		/// The policies that are valid for either this
		/// </summary>
		public IList<StoragePolicy> SupportedPolicies { get; private set; }

		/// <summary>
		/// Size of payload in Bytes
		/// </summary>
		public long StorageSize { get; private set; }

		public ShelfManifest(IList<StoragePolicy> supportedPolicies, long storageSize)
		{
			SupportedPolicies = supportedPolicies;
			StorageSize = storageSize;
		}
	}
}