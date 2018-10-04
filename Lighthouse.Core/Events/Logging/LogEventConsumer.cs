using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Lighthouse.Core.Storage;

namespace Lighthouse.Core.Events.Logging
{
    public class LogEventConsumer : BaseEventConsumer
    {		
		// this is a bit of a hack, to automatically rollover logs daily. 
		private string LOG_NAME { get; set; }
		private IList<StoragePolicy> LoadingDockPolicies => new[] { StoragePolicy.Ephemeral };				
		public List<string> AllLogRecords => LighthouseContainer.Warehouse.Retrieve<string>( new StorageKey(LOG_NAME, this)).ToList();		
		public override IList<Type> Consumes => new[] { typeof(LogEvent) };

		public void HandleEvent(LogEvent logEvent)
		{
			LighthouseContainer.Log(LogLevel.Debug, LogType.EventReceived, this, message: logEvent.ToString());
			LighthouseContainer.Warehouse.Append(new StorageKey(LOG_NAME, this), new[] { $"[{logEvent.Time}] {logEvent.Message}" }, LoadingDockPolicies);			
		}

		protected override void OnInit()
		{
			LOG_NAME = $"{LighthouseContainer.GetNow().ToString("MMddyyyy")}_LOG";
			LighthouseContainer.Warehouse.Store(new StorageKey(LOG_NAME, this), new[] { $"[{LighthouseContainer.GetNow()}] Log Starting" }, LoadingDockPolicies);
		}
	}
}
