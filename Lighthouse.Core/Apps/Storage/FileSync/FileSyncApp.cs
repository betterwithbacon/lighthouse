using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lighthouse.Apps.Storage.FileSync
{
    public class FileSyncFolderStatus
    {
        public string Path { get; }

        public Dictionary<string, DateTime> FileUpdateDate { get; set; } = new Dictionary<string, DateTime>();
    }

    public class FileSyncAppConfig
    {
        public List<string> FoldersToWatch { get; } = new List<string>();
        public string SourceServer { get; set; }
        public string TargetServer { get; set; }
    }

    [ExternalLighthouseService("filesync")]
    public class FileSyncApp : LighthouseServiceBase
    {
        protected readonly Dictionary<string, List<string>> FilesByFolder = new Dictionary<string, List<string>>();
        public string SourceServer { get; private set; }
        public string TargetServer { get; private set; }

        public FileSyncApp()
        {
            // upon startup
        }

        protected override void OnInit(object context = null)
        {
            base.OnInit(context);

            // this probably should either come from the app config or the DB where we store app states
            // right now this starts over, every time it boots up
            if(context is FileSyncAppConfig appConfig)
            {                
                foreach(var folder in appConfig.FoldersToWatch)
                {
                    FilesByFolder.TryAdd(folder, new List<string>());
                }

                SourceServer = appConfig.SourceServer;
                TargetServer = appConfig.TargetServer;
            }
            else
            {
                throw new InvalidOperationException($"file sync app was expecting configuration object and instead got {context} ");
            }
        }

        protected override async Task OnStart()
        {
            static IEnumerable<(string folder, string path)> recurseFolders(IEnumerable<FileSystemObject> objects, string currentFolder)
            {
                foreach (var obj in objects)
                {
                    if (obj.IsDirectory)
                    {
                        foreach (var subobject in recurseFolders(obj.Children, obj.Path))
                            yield return subobject;
                    }
                    else
                    {
                        yield return (currentFolder, obj.Path);
                    }
                }
            }

            // start syncing files
            // request a file manifest from the remote server recursively for all files in the folders
            // record the file statuses in the local databases.

            // local db: C:\test --> C:\test\file1, C:\testfile2
            // remote: --> C:\test\file1, C:\testfile2, *C:\testfile3*
            foreach (var folder in FilesByFolder)
            {
                var response = await Container.MakeRequest<FileSystemRequest, FileSystemLsResponse>(
                    SourceServer, 
                    new FileSystemRequest { 
                        Type = FileSystemRequestType.LS, IsRecursive = true, Folder = folder.Key
                    }
                );

                foreach(var objectStatus in recurseFolders(response.Objects, folder.Key))
                {
                    FilesByFolder.TryAdd(folder.Key, new List<string>());
                    FilesByFolder[folder.Key].Add(objectStatus.path);
                }
            }

            // after all of the files are there, then begin to copy them (in parallel?)

            await Task.CompletedTask;
        }
    }
}
