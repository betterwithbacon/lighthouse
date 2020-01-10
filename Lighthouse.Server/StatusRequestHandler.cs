using Lighthouse.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Server
{
    public class StatusRequestHandler : LighthouseServiceBase, IRequestHandler<StatusRequest, StatusResponse>
    {
        public const string GLOBAL_VERSION_NUMBER = "1.0.0.0";
        public StatusResponse Handle(StatusRequest request)
        {
            return new StatusResponse (
                new Version(GLOBAL_VERSION_NUMBER),// TODO: get version
                Container.ToString(),
                Container.GetNow()
            );
        }
    }

    public class StatusResponse
    {
        public Version Version { get; }
        public string ServerName { get; }
        public DateTime ServerTime { get; }

        public StatusResponse(Version version, string serverName, DateTime serverTime)
        {
            Version = version;
            ServerName = serverName;
            ServerTime = serverTime;
        }

        public override string ToString()
        {
            return $"{ServerName} (version: {Version}): {ServerTime.ToShortTimeString()}";
        }
    }

    public class StatusRequest
    {
    }
}
