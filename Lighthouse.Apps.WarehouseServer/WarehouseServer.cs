using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Events;
using Lighthouse.Core.Storage;
using Lighthouse.Storage;
using System;
using System.Collections.Generic;

namespace Lighthouse.Apps.WarehouseServer
{
    [ExternalLighthouseService("warehouse")]
    public class WarehouseServer : LighthouseServiceBase, IRequestHandler<StorageRequest, StorageResponse>
    {
        public WarehouseServer()
        {
        }

        public IList<Type> Consumes => throw new NotImplementedException();

        public Warehouse Warehouse { get; private set; }

        public StorageResponse Handle(StorageRequest request)
        {
            switch (request.Action)
            {
                case StorageAction.Store:
                    Warehouse.Store(StorageScope.Global, request.Key, request.Data, request.LoadingDockPolicies);
                    return StorageResponse.Stored;
                case StorageAction.Retrieve:
                    Warehouse.Retrieve(StorageScope.Global, request.Key);
                    return new StorageResponse(false, "");
                case StorageAction.Inspect:
                    return new StorageResponse(false, "");
                case StorageAction.Delete:
                    return new StorageResponse(false, "");
                default:
                    return new StorageResponse(false, "");
            }
        }

        protected override void OnStart()
        {
            // locate persistent data stores

            // start scheduled task that does the syncing

            // begin listening for storage events
            
        }
    }

    public class StorageResponse
    {
        public static StorageResponse Stored = new StorageResponse();
        
        public StorageResponse(bool wasSuccessful = true, string message = null)
        {
            WasSuccessful = wasSuccessful;
            Message = message;
        }

        public bool WasSuccessful { get; }
        public string Message { get; }
    }

    public class StorageRequest
    {
        public StorageAction Action { get; set; }
        public byte[] Data { get; set; }
        public string Key { get; set; }
        public IEnumerable<StoragePolicy> LoadingDockPolicies { get; set; }
    }

    public enum StorageAction
    {
        Store,
        Retrieve,
        Delete,
        Inspect
    }
}
