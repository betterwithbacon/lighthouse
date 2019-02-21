using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Lighthouse.Core.Storage;

namespace Lighthouse.Core.Events.Logging
{
    public class LogEventConsumer : BaseEventConsumer
    {	
		private IList<StoragePolicy> LoadingDockPolicies => new[] { StoragePolicy.Ephemeral };
        public List<string> AllLogRecords => new List<string>(); // Container.Warehouse.Retrieve<List<string>>( new StorageKey(LOG_NAME, this)).ToList();		
		public override IList<Type> Consumes => new[] { typeof(LogEvent) };

		public void HandleEvent(LogEvent logEvent)
		{
			Container.Warehouse.Store(this, GetLongTimeName(), new[] { $"[{logEvent.EventTime}] {logEvent.Message}" });
		}

        protected override void OnInit()
        {
            base.OnInit();

            Identifier = "Global Logger";
        }

        private string GetLongTimeName()
        {
            return Container.GetNow().Ticks.ToString();
        }
    }
}
