using System;
using System.Collections.Generic;

namespace Lighthouse.Core.Storage
{
	public class Receipt
	{
		public Guid UUID { get; set; }
		public string Key { get; set; }
		public IList<StoragePolicy> Policies { get; set; }
		public IStorageScope Scope { get; internal set; }
		public string SHA256Checksum { get; set; }
		public bool WasSuccessful { get; private set; }

		public Receipt(bool wasSuccessful)
		{
			WasSuccessful = wasSuccessful;
		}
	}
}