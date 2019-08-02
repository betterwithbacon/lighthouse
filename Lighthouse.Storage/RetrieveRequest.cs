using Lighthouse.Core.Storage;

namespace Lighthouse.Storage
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