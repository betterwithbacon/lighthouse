using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Events;
using System;
using System.Collections.Generic;

namespace Lighthouse.Apps.WarehouseServer
{
    [ExternalLighthouseService("warehouse")]
    public class WarehouseServer : LighthouseServiceBase, IEventConsumer
    {
        public WarehouseServer()
        {
        }

        public IList<Type> Consumes => throw new NotImplementedException();

        public void HandleEvent(IEvent ev)
        {
            if(ev is StorageEvent storageEvent)
            {

            }
        }

        public void Init(ILighthouseServiceContainer container)
        {            
        }

        protected override void OnStart()
        {
            // locate persistent data stores

            // start scheduled task that does the syncing

            // begin listening for storage events
            
        }
    }

    public class StorageEvent : BaseEvent
    {
        public StorageEvent(ILighthouseServiceContainer container, DateTime? eventTime = null) 
            : base(container, eventTime)
        {
        }

        public ILighthouseServiceContainer LighthouseContainer => throw new NotImplementedException();

        public DateTime EventTime => throw new NotImplementedException();
    }
}
