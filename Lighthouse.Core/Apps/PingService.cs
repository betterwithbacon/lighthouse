using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Logging;
using System;
using System.Threading.Tasks;

namespace Lighthouse.Core.Apps
{
    [ExternalLighthouseService("ping")]
    public class PingService : ILighthouseService
    {
        public string Id => "ping";

        public ILighthouseServiceContainer Container { get; private set; }

        public void Initialize(ILighthouseServiceContainer container)
        {
            Container = container;
        }

        public async Task Start()
        {
            await Task.Run(() => {
                    Container.Log(LogLevel.Info, LogType.Info, this, $"ping: time: {Container.GetNow()}. Container Time: {Container.GetNow()}");
                }
            );
        }

        public async Task Stop()
        {
            await Container.RemoveScheduledActions(this);
        }
    }
}