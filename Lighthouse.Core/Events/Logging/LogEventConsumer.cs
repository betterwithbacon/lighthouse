using Lighthouse.Core.Logging;
using Lighthouse.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lighthouse.Storage;

namespace Lighthouse.Core.Events.Logging
{
    public class LogEventConsumer : BaseEventConsumer
    {
		private IWarehouse Warehouse { get; set; }

		// this is a bit of a hack, to automatically rollover logs daily. 
		private string LOG_NAME => $"{LighthouseContainer.GetNow().ToString("MMddyyyy")}_LOG";
		private IList<LoadingDockPolicy> LoadingDockPolicies => new[] { LoadingDockPolicy.Ephemeral };		
		
		public List<string> AllLogRecords => Warehouse.Retrieve<string>( new WarehouseKey(LOG_NAME, this)).ToList();
		
		public override IList<Type> Consumes => new[] { typeof(LogEvent) };

		public void HandleEvent(LogEvent ev)
		{
			this.ThrowIfInvalidEvent(ev);
			
			if (ev is LogEvent logEvent)
			{
				// record that a log event was received
				LighthouseContainer.Log(LogLevel.Debug, LogType.EventReceived, this, message: logEvent.ToString());
				Warehouse.Append(new WarehouseKey(LOG_NAME, this), new[] { $"[{logEvent.Time}] {logEvent.Message}" }, LoadingDockPolicies);
			}
		}

		protected override void OnInit()
		{				

			Warehouse = LighthouseContainer.ResolveService<Warehouse>();			
			Warehouse.Store(new WarehouseKey(LOG_NAME, this), new[] { $"[{LighthouseContainer.GetNow()}] Log Starting" }, LoadingDockPolicies);
		}
	}
}
