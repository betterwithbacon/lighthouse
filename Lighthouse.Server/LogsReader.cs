using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Hosting;

namespace Lighthouse.Server
{
    [ExternalLighthouseService("logs")]
    public class LogsReader : LighthouseServiceBase, ILighthouseServiceHasState
    {
        readonly ConcurrentBag<string> Log = new ConcurrentBag<string>();

        protected override Task OnStart()
        {
            //TODO: act as a logger, but it would be great, if we could read the log that's already been made
            Container.AddLogger(Log.Add);
            return Task.CompletedTask;
        }

        public IEnumerable<string> GetState()
        {
            return Log;
        }
    }

    [ExternalLighthouseService("services")]
    public class ServicesReader : LighthouseServiceBase, ILighthouseServiceHasState
    {
        public IEnumerable<string> GetState()
        {
            return Container.GetRunningServices().Select(s => s.ExternalServiceName()).ToList();
        }
    }
}
