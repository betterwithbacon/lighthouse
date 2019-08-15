using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.IO
{
	public static class FileSystemProviderExtensions
	{
		public static string ReadStringFromFileSystem(this IFileSystemProvider provider, string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));

			var content = provider?.ReadFromFileSystem(fileName).GetAwaiter().GetResult();
			if (content != null)
			{
				return Encoding.UTF8.GetString(content);
			}

			// iof the content is null, return null
			return null;
		}

		public static void WriteStringToFileSystem(this IFileSystemProvider provider, string fileName, string data)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));

			// you can write empty strings but not nulls
			if (data == null)
				return;

			provider.WriteToFileSystem(fileName, Encoding.UTF8.GetBytes(data));
		}
	}
}
