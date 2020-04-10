using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.IO
{
	public class VirtualFileSystem : IFileSystemProvider
	{
		public ConcurrentDictionary<string, byte[]> Data = new ConcurrentDictionary<string, byte[]>();

		public ResourceProviderType Type => ResourceProviderType.FileSystem;

		public async Task AppendToFileOnFileSystem(string fileName, byte[] data)
		{
			await Task.Run(() => WriteToFileSystem(fileName, data));
		}

		public bool FileExists(string fileName)
		{
			return Data.ContainsKey(fileName);
		}

		public async Task<byte[]> ReadFromFileSystem(string fileName)
		{
			return await Task.FromResult(Data.TryGetValue(fileName, out var data) ? data : null);
		}

		public void Register(ILighthouseServiceContainer peer, Dictionary<string, string> otherConfig = null)
		{
			// do nothing?
		}

		public void WriteToFileSystem(string fileName, byte[] data)
		{
			Data.AddOrUpdate(fileName, data, (key, existing) => data);
		}
	}
}
