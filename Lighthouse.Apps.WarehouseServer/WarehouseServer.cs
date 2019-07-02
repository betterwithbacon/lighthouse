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
        public WarehouseServer(IWarehouse warehouse)
        {
            Warehouse = warehouse;
        }

        public IList<Type> Consumes => throw new NotImplementedException();

        public IWarehouse Warehouse { get; private set; }

        public StorageResponse Handle(StorageRequest request)
        {
            if(Warehouse == null)
            {
                throw new ApplicationException("No warehouse is present");
            }

            switch (request.Action)
            {
                case StorageAction.Store:
                    return Store(request);
                case StorageAction.Retrieve:
                    return Retrieve(request);                    
                case StorageAction.Inspect:
                    return Inspect(request);
                case StorageAction.Delete:
                    return Delete(request);
                default:
                    return new StorageResponse(false, "");
            }
        }

        private StorageResponse Delete(StorageRequest request)
        {
            if (request.PayloadType == StoragePayloadType.Blob)
                Warehouse.Store(StorageScope.Global, request.Key, null, request.LoadingDockPolicies);
            else
                Warehouse.Store(StorageScope.Global, request.Key, null, request.LoadingDockPolicies);

            return new StorageResponse();
        }

        private StorageResponse Inspect(StorageRequest request)
        {
            var manifest = Warehouse.GetManifest(StorageScope.Global, request.Key);

            return new StorageResponse
            {
                Manifest = manifest
            };
        }

        private StorageResponse Retrieve(StorageRequest request)
        {
            string value = Warehouse.Retrieve(StorageScope.Global, request.Key);

            return new StorageResponse
            {
                StringData = value
            };
        }

        private StorageResponse Store(StorageRequest request)
        {
            Receipt receipt = null;
            if (request.PayloadType == StoragePayloadType.Blob)
                receipt = Warehouse.Store(StorageScope.Global, request.Key, request.Data, request.LoadingDockPolicies);
            else
                receipt = Warehouse.Store(StorageScope.Global, request.Key, request.StringData, request.LoadingDockPolicies);

            return new StorageResponse
            {
                Receipt = receipt
            };
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
        public Receipt Receipt { get; internal set; }
        public byte[] Data { get; set; }
        public string StringData { get; set; } // TODO: is this a necessary hack?!
        public StorageKeyManifest Manifest { get; internal set; }
    }

    public class StorageRequest
    {
        public StoragePayloadType PayloadType { get; set; }
        public StorageAction Action { get; set; }
        public byte[] Data { get; set; }
        public string StringData { get; set; } // TODO: is this a necessary hack?!
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

    public enum StoragePayloadType
    {
        String,
        Blob
    }
}
