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
			StatusUpdated?.Invoke(this, $"RootDirectory is {RootDirectory}");
		}

		public event StatusUpdatedEventHandler StatusUpdated;

		public async Task<byte[]> ReadFromFileSystem(string fileName)
		{
			var fullWritePath = GetFilePath(fileName);
			StatusUpdated(this, $"[READ] Requested for {fullWritePath}.");
			return await File.ReadAllBytesAsync(fullWritePath);
		}

		public void WriteToFileSystem(string fileName, byte[] data)
		{
			var fullWritePath = GetFilePath(fileName);
			StatusUpdated(this, $"[WRITE] Requested for {fileName}. Writing to {fullWritePath}");

			EnsurePath(fullWritePath);
			File.WriteAllBytes(fullWritePath, data);
		}

		private void EnsurePath(string fullWritePath)
		{
			new FileInfo(fullWritePath).Directory.Create();
		}

		public bool FileExists(string fileName)
		{
			var fullWritePath = GetFilePath(fileName);
			StatusUpdated(this, $"[FILE_EXISTS] Requested for {fullWritePath}.");
			return File.Exists(fullWritePath);
		}

		string GetFilePath(string fileName)
		{
			return Path.Combine(RootDirectory, fileName.TrimStart('\\'));
		}

		public override string ToString()
		{
			return "Windows File System Provider";
		}

		public async Task AppendToFileOnFileSystem(string fileName, byte[] data)
		{
			var fullyQualifiedPathName = GetFilePath(fileName);

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
