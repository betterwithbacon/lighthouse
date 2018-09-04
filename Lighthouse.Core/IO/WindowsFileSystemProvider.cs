using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.IO
{
	public class WindowsFileSystemProvider : IFileSystemProvider
	{
		public byte[] ReadFromFileSystem(string fileName)
		{
			return null;	
		}

		public void WriteToFileSystem(string fileName, byte[] data)
		{
			
		}
	}
}
