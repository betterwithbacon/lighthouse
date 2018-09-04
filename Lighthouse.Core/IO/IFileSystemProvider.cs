using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.IO
{
	public interface IFileSystemProvider : IResourceProvider
	{
		/// <summary>
		/// Provides abstracted access to the runtime file system. This might be either a linux or window host. 
		/// The storage might also not be truly persistent, if the container is hosted on an ephemeral file system.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		void WriteToFileSystem(string fileName, byte[] data);

		/// <summary>
		/// Provides abstracted access to the runtime file system. This might be either a linux or window host. 
		/// The storage might also not be truly persistent, if the container is hosted on an ephemeral file system.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		byte[] ReadFromFileSystem(string fileName);
	}
}
