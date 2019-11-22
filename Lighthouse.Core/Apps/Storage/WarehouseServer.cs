using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Events;
using Lighthouse.Core.Storage;
using Lighthouse.CORE.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lighthouse.Apps.WarehouseServer
{
    //[ExternalLighthouseService("warehouse")]
    //public class WarehouseServer : LighthouseServiceBase, IRequestHandler<StorageRequest, StorageResponse>
    //{
    //    public WarehouseServer(IEnumerable<IWarehouse> warehouses)
    //    {
    //        Warehouses = new ConcurrentBag<IWarehouse>();
    //        foreach (var warehouse in warehouses)
    //        {
    //            Warehouses.T = warehouses;
    //        }
    //    }

    //    public ConcurrentBag<IWarehouse> Warehouses { get; private set; } = new ConcurrentBag<IWarehouse>();

    //    public StorageResponse Handle(StorageRequest request)
    //    {  
    //        switch (request.Action)
    //        {
    //            case StorageAction.Store:
    //                return Store(request);
    //            case StorageAction.Retrieve:
    //                return Retrieve(request);                    
    //            case StorageAction.Inspect:
    //                return Inspect(request);
    //            case StorageAction.Delete:
    //                return Delete(request);
    //            default:
    //                return new StorageResponse(false, "");
    //        }
    //    }

    //    private StorageResponse Delete(StorageRequest request)
    //    {
    //        if (request.PayloadType == StoragePayloadType.Blob)
    //            Warehouse.Store(StorageScope.Global, request.Key, null, request.LoadingDockPolicies);
    //        else
    //            Warehouse.Store(StorageScope.Global, request.Key, null, request.LoadingDockPolicies);

    //        return new StorageResponse();
    //    }

    //    private StorageResponse Inspect(StorageRequest request)
    //    {
    //        var manifest = Warehouse.GetManifest(StorageScope.Global, request.Key);

    //        return new StorageResponse
    //        {
    //            Manifest = manifest
    //        };
    //    }

    //    private StorageResponse Retrieve(StorageRequest request)
    //    {
    //        string value = Warehouse.Retrieve(StorageScope.Global, request.Key);

    //        return new StorageResponse
    //        {
    //            StringData = value
    //        };
    //    }

    //    private StorageResponse Store(StorageRequest request)
    //    {
    //        Receipt receipt = null;
    //        if (request.PayloadType == StoragePayloadType.Blob)
    //            receipt = Warehouse.Store(StorageScope.Global, request.Key, request.Data, request.LoadingDockPolicies);
    //        else
    //            receipt = Warehouse.Store(StorageScope.Global, request.Key, request.StringData, request.LoadingDockPolicies);

    //        return new StorageResponse
    //        {
    //            Receipt = receipt
    //        };
    //    }

    //    protected override void OnStart()
    //    {
    //        // locate persistent data stores
    //        // a warehouse server acts as a node, communicating with other single-storage mode data stores...
    //        // ...as well as other warehouse servers. Warehouse servers, nominally receive communication from other machines
    //        var configuration = Container.GetConfigurationProvider().GetConfiguration<WarehouseServerConfig>(this.Identifier);
            
    //        // start scheduled task that does the syncing
    //        // the warehouse server, should talk to it's warehouses, and do the movement between warehouse nodes

    //        // begin listening for storage events

    //    }

    //    public List<WarehouseServerConnection> Connections { get; set; }
    //}

    //public class WarehouseServerConfig
    //{
    //    public List<WarehouseServerConnection> Connections { get; set; }
    //}

    //public class WarehouseServerConnection
    //{
    //    public WarehouseServerConnectionType Type { get; set; }
    //    public string ConnectionString { get; set; }
    //}


    
}
