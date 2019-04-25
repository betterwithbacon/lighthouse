using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Services
{
    [ExternalLighthouseService("ping")]
    public class PingService : ILighthouseService
    {
        public string Id => throw new NotImplementedException();

        public LighthouseServiceRunState RunState => throw new NotImplementedException();

        public ILighthouseServiceContainer Container => throw new NotImplementedException();

        public void Initialize(ILighthouseServiceContainer context)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
