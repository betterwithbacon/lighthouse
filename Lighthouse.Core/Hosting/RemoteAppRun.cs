using System;

namespace Lighthouse.Core.Hosting
{
    public static class RemoteAppRunStatus
    {
        public const string Succeeded = "succeeded";
        public const string Failed = "failed";
    }

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
        public string Status { get; set; } = RemoteAppRunStatus.Succeeded;

        public RemoteAppRunHandle(string id = null)
        {
            Id = id ?? Guid.NewGuid().ToString();
        }
    }
}