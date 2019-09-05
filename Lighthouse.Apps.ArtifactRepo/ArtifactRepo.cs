using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lighthouse.Core;
using Lighthouse.Core.Storage;

namespace Lighthouse.Apps.ArtifactRepo
{
    public class ArtifactRepo : LighthouseServiceBase
    {
        public ArtifactRepoManifest Manifest { get; private set; }

        protected override Task OnStart()
        {
            
            // Load the manifest that connects the metadata to the persistent blob store
            Manifest = new ArtifactRepoManifest();

            var dictionary = Container
                .Warehouse
                .Retrieve<Dictionary<string,string>>(StorageScope.Global, ArtifactRepoManifest.StorageKey);

                manifest.Load(dictionary);
        }
    }

    public class ArtifactRepoManifest
    {
        public const string StorageKey = "artifact_repo_manifest";
    }

    public class BlobStore : LighthouseServiceBase
    {

    }
}
