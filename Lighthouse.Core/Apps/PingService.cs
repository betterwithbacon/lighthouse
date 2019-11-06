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
            await Container.AddScheduledAction(this, (time) => Container.Log(LogLevel.Info, LogType.Info, this, $"ping: time: {time}. Container Time: {Container.GetNow()}"),
                minuteFrequency: 1);
        }

        public async Task Stop()
        {
            await Container.RemoveScheduledActions(this);
        }
    }
}