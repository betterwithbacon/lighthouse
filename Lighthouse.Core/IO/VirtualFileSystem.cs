using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.IO
{
	public class VirtualFileSystem : IFileSystemProvider
	{
		// c:\folder
		// C:\folder\file.txt
		// C:\folder\sub_folder
		// C:\folder\sub_folder\file2.txt
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

		public IEnumerable<FileSystemObject> GetObjects(string folder)
		{
			return Data
				.Where(kvp => kvp.Key.StartsWith(folder))
				.Select(d => new FileSystemObject{
					Path = d.Key,
					IsDirectory = d.Key.EndsWith('\\'), // TODO: HUGE hack, this should be smarter
					// Children = Data.Keys.Where(k => k.StartsWith(folder)).Where(f => f.EndsWith(('\\'))).ToList()
				});
		}

		public async Task<byte[]> ReadFromFileSystem(string fileName)
		{
			return await Task.FromResult(new byte[0]);
			//return await Task.FromResult(Data.TryGetValue(fileName, out var data) ? Data : null);
		}

		public void Register(ILighthouseServiceContainer peer, Dictionary<string, string> otherConfig = null)
		{
			// do nothing?
			// default to C drive I guess?!

		}

		public void WriteToFileSystem(string fileName, byte[] data)
		{
			//Data.AddOrUpdate(fileName, data, (key, existing) => data.Data);
		}

		// public class FileSystemDataObject : FileSystemObject
		// {
		// 	public byte[] Data { get; set; }
		// }
	}

	public static class FileSystemObjectExtensions
	{
		public static FileSystemObject Find(this FileSystemObject searchRoot, string folder)
		{
			if (searchRoot.Path == folder)
				return searchRoot;
			
			foreach(var child in searchRoot.Children)
			{
				var val = child.Find(folder);
				if (val != null)
					return val;
			}

			return null;
		}
	}
}
