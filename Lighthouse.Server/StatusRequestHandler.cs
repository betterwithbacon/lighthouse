using Lighthouse.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Server
{
    public class StatusRequestHandler : LighthouseServiceBase, IRequestHandler<StatusRequest, StatusResponse>
    {
        public StatusResponse Handle(StatusRequest request)
        {
            return new StatusResponse (
                new Version("0.0.0.0"),// TODO: get version
                Container.ServerName,
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
    }

    public class StatusRequest
    {
    }
}
