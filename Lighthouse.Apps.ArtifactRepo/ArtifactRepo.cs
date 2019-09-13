using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lighthouse.Core;
using Lighthouse.Core.Storage;

namespace Lighthouse.Apps.ArtifactRepo
{
    public class ArtifactRepo : LighthouseServiceBase
    {
        public ArtifactRepoManifest Manifest { get; private set; }

        protected override async Task OnStart()
        {
            
            // Load the manifest that connects the metadata to the persistent blob store
            Manifest = new ArtifactRepoManifest();

            var dictionary = Container
                .Warehouse
                .Retrieve<Dictionary<string,string>>(StorageScope.Global, ArtifactRepoManifest.StorageKey);

            Manifest.Load(dictionary);

            await Task.CompletedTask;
        }
    }

    public class ArtifactRepoManifest
    {
        public const string StorageKey = "artifact_repo_manifest";

        private ConcurrentDictionary<string, string> Values { get; set; } = new ConcurrentDictionary<string, string>();

        internal void Load(Dictionary<string, string> dictionary)
        {
            Values = new ConcurrentDictionary<string, string>(dictionary);
        }
    }

    public class BlobStore : LighthouseServiceBase
    {

    }
}
