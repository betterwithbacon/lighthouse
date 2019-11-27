namespace Lighthouse.Core.Hosting
{
    public class RemoteAppRunRequest
    {
        public string What { get; set; }

        public RemoteAppRunRequest(string what)
        {
            this.What = what;
        }
    }

    public class RemoteAppRunHandle
    {
        public string Id { get; set; }
        public string Status { get; set; }

        public RemoteAppRunHandle(string id)
        {
            Id = id;
        }
    }
}