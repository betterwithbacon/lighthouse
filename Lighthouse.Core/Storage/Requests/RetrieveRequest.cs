using Lighthouse.Core.Storage;

namespace Lighthouse.Core.Storage.Requests
{
    public class RetrieveRequest : StorageRequest
    {
        public string Key { get; set; }
    }

    public class RetrieveResponse
    {
        public string Data { get; set; }
    }
}