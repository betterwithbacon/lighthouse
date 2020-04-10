using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
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
        protected readonly ConcurrentDictionary<string, List<FileSyncFolderStatus>> FolderStatuses = new ConcurrentDictionary<string, List<FileSyncFolderStatus>>();
        public string SourceServer { get; private set; }
        public string TargetServer { get; private set; }

        public FileSyncApp()
        {
            // upon startup
        }

        protected override void OnInit(object context = null)
        {
            base.OnInit(context);

            if(context is FileSyncAppConfig appConfig)
            {
                foreach(var folder in appConfig.FoldersToWatch)
                {
                    FolderStatuses.TryAdd(folder, new List<FileSyncFolderStatus>());
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
            // start syncing files
            // request a file manifest from the remote server recursively for all files in the folders
            // record the file statuses in the local databases.
            foreach (var folder in FolderStatuses)
            {
                // Container.HandleRequest(new FileSystemRequest { Type=FileSystemRequestType.LS, IsRecurisive=true,  });
            }

            // after all of the files are there, then begin to copy them (in parallel?)

            await Task.CompletedTask;
        }
    }
}
