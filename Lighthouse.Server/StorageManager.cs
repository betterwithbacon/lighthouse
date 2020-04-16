using Lighthouse.Core;
using Lighthouse.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lighthouse.Server
{
	public class StorageManager : LighthouseServiceBase, IRequestHandler<FileSystemRequest, FileSystemResponse>
	{
		public FileSystemResponse Handle(FileSystemRequest request)
		{
			switch(request.Type)
			{
				case FileSystemRequestType.LS:
					return new FileSystemResponse { Objects = ListFiles(request.Folder).ToList() };
			}

			return new FileSystemResponse();
		}

		private IEnumerable<FileSystemObject> ListFiles(string folder)
		{
			// this request, needs to be routed to the container
			var filesystem = Container.GetFileSystem();
							
			if(filesystem == null)
			{
				return Enumerable.Empty<FileSystemObject>();
			}
			else
			{
				return filesystem.GetObjects(folder);
			}
		}
	}
}
