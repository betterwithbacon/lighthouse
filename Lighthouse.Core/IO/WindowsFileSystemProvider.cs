using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.IO
{
	public class WindowsFileSystemProvider : IFileSystemProvider
	{
		public string RootDirectory { get; }

		public ILighthouseServiceContainer LighthouseContainer { get; }

		public WindowsFileSystemProvider(string rootDirectory, ILighthouseServiceContainer container)
		{
			if (string.IsNullOrEmpty(rootDirectory))
			{
				throw new ArgumentException("storageRootDirectory must be specified", nameof(rootDirectory));
			}

			RootDirectory = rootDirectory;
			LighthouseContainer = container;
		}

		public event StatusUpdatedEventHandler StatusUpdated;

		public async Task<byte[]> ReadFromFileSystem(string fileName)
		{
			StatusUpdated(this, $"[READ] Requested for {fileName}.");
			return await File.ReadAllBytesAsync(Path.Combine(RootDirectory, fileName));
		}

		public async Task WriteToFileSystem(string fileName, byte[] data)
		{
			StatusUpdated(this, $"[WRITE] Requested for {fileName}.");
			await File.WriteAllBytesAsync(Path.Combine(RootDirectory, fileName), data);
		}

		public bool FileExists(string fileName)
		{
			StatusUpdated(this, $"[FILE_EXISTS] Requested for {fileName}.");
			return File.Exists(Path.Combine(RootDirectory, fileName));
		}

		public override string ToString()
		{
			return "Windows File System Provider";
		}

		public async Task AppendToFileOnFileSystem(string fileName, byte[] data)
		{
			var fullyQualifiedPathName = Path.Combine(RootDirectory, fileName);

			await Task.Run(() =>
			{
				using (var filestream = new FileStream(fullyQualifiedPathName, 
					FileExists(fullyQualifiedPathName) ?  FileMode.Append : FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite))
				{
					filestream.Write(data, 0, data.Length);
				}
			});
		}
	}
}
