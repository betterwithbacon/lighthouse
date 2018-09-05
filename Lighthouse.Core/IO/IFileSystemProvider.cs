using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.IO
{
	/// <summary>
	/// Provides access to a local file system. This access neither exhaustive nor complete. But rather provides limited abstractions to access the underlying OS  that is running the lighthouse environment.
	/// File paths should be provided as non-root relative paths. The virtual "root" of the provider will be somewhere else.
	/// </summary>
	public interface IFileSystemProvider : IResourceProvider
	{
		/// <summary>
		/// Provides abstracted access to the runtime file system. This might be either a linux or window host. 
		/// The storage might also not be truly persistent, if the container is hosted on an ephemeral file system.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		Task WriteToFileSystem(string fileName, byte[] data);

		/// <summary>
		/// Provides abstracted access to the runtime file system. This might be either a linux or window host. 
		/// The storage might also not be truly persistent, if the container is hosted on an ephemeral file system.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		Task<byte[]> ReadFromFileSystem(string fileName);

		/// <summary>
		/// Allows Data to be appended to an existing file
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		Task AppendToFileOnFileSystem(string fileName, byte[] data);

		/// <summary>
		/// Indicates whether a file exists on the file system
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		bool FileExists(string fileName);
	}

	/// <summary>
	/// A very rudimentary interface for Unix-like file systems. This abstraction makes no distinction about the underlying file systems (ZFS vs EXT vs Brts, etc).
	/// </summary>
	public class UnixFileSystemProvider : IFileSystemProvider
	{
		public ILighthouseServiceContainer LighthouseContainer => throw new NotImplementedException();

		public event StatusUpdatedEventHandler StatusUpdated;

		public bool FileExists(string fileName)
		{
			throw new NotImplementedException();
		}

		public Task<byte[]> ReadFromFileSystem(string fileName)
		{
			throw new NotImplementedException();
		}

		public Task WriteToFileSystem(string fileName, byte[] data)
		{
			throw new NotImplementedException();
		}
	}
}
