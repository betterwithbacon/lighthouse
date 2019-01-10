using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Lighthouse.Core.Storage;

namespace Lighthouse.Core.Events.Logging
{
    public class LogEventConsumer : BaseEventConsumer
    {
		private string LOG_NAME { get; set; }
		private IList<StoragePolicy> LoadingDockPolicies => new[] { StoragePolicy.Ephemeral };				
		public List<string> AllLogRecords => Container.Warehouse.Retrieve<string>( new StorageKey(LOG_NAME, this)).ToList();		
		public override IList<Type> Consumes => new[] { typeof(LogEvent) };

		public void HandleEvent(LogEvent logEvent)
		{
			Container.Warehouse.Append(new StorageKey(LOG_NAME, this), new[] { $"[{logEvent.EventTime}] {logEvent.Message}" }, LoadingDockPolicies);
		}

		protected override void OnInit()
		{
			// this is a bit of a hack, to automatically rollover logs daily. 
			LOG_NAME = $"{Container.GetNow().ToString("MMddyyyy")}_LOG";
			Container.Warehouse.Store(new StorageKey(LOG_NAME, this), new[] { $"[{Container.GetNow()}] Log Starting" }, LoadingDockPolicies);
		}
	}
}
