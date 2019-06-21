using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.Services
{
    [ExternalLighthouseService("ping")]
    public class PingService : ILighthouseService
    {   
        public string Id => "ping";
        private const string ScheduleName = "PingSchedule";

        public ILighthouseServiceContainer Container { get; private set; }

        public void Initialize(ILighthouseServiceContainer container)
        {
            Container = container;
        }

        public void Start()
        {
            Container.AddScheduledAction(new Scheduling.Schedule(Scheduling.ScheduleFrequency.Secondly, 30, name: ScheduleName), 
                (time) => Container.Log(LogLevel.Info, LogType.Info, this, $"ping: time: {time}. Container Time: {Container.GetNow()}"));            
        }

        public void Stop()
        {
            Container.RemoveScheduledAction(ScheduleName);
        }
    }
}
