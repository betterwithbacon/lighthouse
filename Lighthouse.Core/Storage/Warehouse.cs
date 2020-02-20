﻿using System;
using System.Collections.Concurrent;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Storage.Legacy.Requests;
using Lighthouse.Core.Utils;

namespace Lighthouse.Core.Storage
{
    // for now we're making a MASSIVE assumption, about the storage. the idea is that there's essentially one storage warehouse
    // scoped to the entire cluster and potentially beyond. 
    [ExternalLighthouseService("warehouse")]
    public class Warehouse : LighthouseServiceBase,
                               IRequestHandler<WarehouseStoreRequest, bool>,
                               IRequestHandler<WarehouseRetrieveRequest, WarehouseRetrieveResponse>
    {
        private ConcurrentDictionary<string, string> Data { get; set; } = new ConcurrentDictionary<string, string>();

        public void Store(string key, string data)
        {
            _ = Data.AddOrUpdate(key, data, (k, v) => data);
        }

        public string Retrieve(string key)
        {
            // first hit local version
            // this does absolutely nothing for cache invalidation

            if(!Data.TryGetValue(key, out var val))
            {
                // talk to other warehouses
                foreach(var peer in Container.GetPeers())
                {
                    var returnedval = peer.HandleRequest<WarehouseRetrieveRequest, WarehouseRetrieveResponse>(new WarehouseRetrieveRequest
                    {
                        Key = key
                    }).GetAwaiter().GetResult();
                    if (returnedval.Value != null)
                    {
                        val = returnedval.Value;
                        break;
                    }
                }
            }

            return val;
        }

        public bool Handle(WarehouseStoreRequest request)
        {
            Store(request.Key, request.Value);            
            return true;
        }

        public WarehouseRetrieveResponse Handle(WarehouseRetrieveRequest request)
        {
            return new WarehouseRetrieveResponse
            {
                Value = Retrieve(request.Key)
            };
        }
    }

    
    public class WarehouseStoreRequest
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class WarehouseRetrieveRequest
    {
        public string Key { get; set; }
    }

    public class WarehouseRetrieveResponse
    {
        public string Value { get; set; }
    }

    public static class WarehouseExtensions
    {
        public static T Retrieve<T>(this Warehouse warehouse, string key)
        {
            var stringVal = warehouse.Retrieve(key);

            if (stringVal == null)
                return default;

            return stringVal.DeserializeFromJSON<T>();
        }
    }
}
