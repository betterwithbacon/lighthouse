using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Logging;
using System;

namespace Lighthouse.Apps.Core
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

        public void Start()
        {
            Container.AddScheduledAction(this, (time) => Container.Log(LogLevel.Info, LogType.Info, this, $"ping: time: {time}. Container Time: {Container.GetNow()}"),
                minuteFrequency: 1).GetAwaiter().GetResult();
        }

        public void Stop()
        {
            Container.RemoveScheduledActions(this);
        }
    }
}
