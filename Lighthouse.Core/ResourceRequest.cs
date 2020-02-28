namespace Lighthouse.Core
{
    public class ResourceRequest
    {
        public ResourceProviderType ResourceType { get; set; }
        public ResourceRequestType RequestType { get; set; }
        public ResourceProviderConfig Config { get; set; }
    }

    public enum ResourceRequestType
    {
        Add,
        Remove,
        Inspect
    }
}
