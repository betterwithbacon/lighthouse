using System;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Storage.Legacy.Requests;
using Lighthouse.Core.Utils;

namespace Lighthouse.Core.Storage
{
    [ExternalLighthouseService("warehouse")]
    public class Warehouse : LighthouseServiceBase,
                               IRequestHandler<WarehouseStoreRequest, bool>,
                               IRequestHandler<WarehouseRetrieveRequest, RetrieveResponse>
    {
        public void Store(string key, string data)
        {
        }

        public string Retrieve(string key)
        {
            return null;
        }

        public bool Handle(WarehouseStoreRequest request)
        {
            throw new NotImplementedException();
        }

        public RetrieveResponse Handle(WarehouseRetrieveRequest request)
        {
            throw new NotImplementedException();
        }
    }

    public class WarehouseStoreRequest
    {

    }

    public class WarehouseRetrieveRequest
    {

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
