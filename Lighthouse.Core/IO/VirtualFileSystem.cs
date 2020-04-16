using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
		//public ConcurrentDictionary<string, FileSystemDataObject> Data = new ConcurrentDictionary<string, FileSystemDataObject>();
		public FileSystemDataObject Root { get; set; }

		public ResourceProviderType Type => ResourceProviderType.FileSystem;

		public async Task AppendToFileOnFileSystem(string fileName, byte[] data)
		{
			await Task.Run(() => WriteToFileSystem(fileName, data));
		}

		public bool FileExists(string fileName)
		{
			return Root 
		}

		public IEnumerable<FileSystemObject> GetObjects(string folder)
		{
			yield return Data[folder];
		}

		public async Task<byte[]> ReadFromFileSystem(string fileName)
		{
			return await Task.FromResult(Data.TryGetValue(fileName, out var data) ? data.Data : null);
		}

		public void Register(ILighthouseServiceContainer peer, Dictionary<string, string> otherConfig = null)
		{
			// do nothing?
			// default to C drive I guess?!

		}

		public void WriteToFileSystem(string fileName, byte[] data)
		{
			Data.AddOrUpdate(fileName, data, (key, existing) = >data.Data);
		}

		public class FileSystemDataObject : FileSystemObject
		{
			public byte[] Data { get; set; }
		}
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
