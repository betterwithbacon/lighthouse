using System;
using System.Collections.Generic;
using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Hosting;

namespace Lighthouse.Server
{
    [ExternalLighthouseService("logs")]
    public class LogsReader : LighthouseServiceBase, ILighthouseServiceHasState
    {
        public IEnumerable<string> GetState()
        {
             
        }
    }
}
