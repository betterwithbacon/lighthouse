using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WarehouseCore;

namespace Lighthouse.Core.Events.Logging
{
    public class LogEventConsumer : IEventConsumer, ILighthouseLogSource, IStorageScope
    {
		private Warehouse Warehouse { get; set; }
		private string LOG_NAME => $"{Context.GetTimeNow().ToString("MMddyyyy")}_LOG";		
		public IList<Type> Consumes => new[] { typeof(LogEvent) };
		private IList<LoadingDockPolicy> LoadingDockPolicies => new[] { LoadingDockPolicy.Ephemeral };
		public string Identifier { get; private set; }
		IEventContext Context { get; set; }		
		public string ScopeName => "EventLogger_1";

		public List<string> LogLines => Warehouse.Retrieve(LOG_NAME, this).ToList();

		public void HandleEvent(IEvent ev)
		{
			this.ThrowIfInvalidEvent(ev);
			var logEvent = ev as LogEvent;

			Context.Log(LogType.EventReceived, logEvent.ToString(), this);
			Warehouse.Append(LOG_NAME, this, new[] { $"[{logEvent.Time}] {logEvent.Message}" }, LoadingDockPolicies);
		}

		public void Init(IEventContext context)
		{
			Context = context;
			Identifier = EventContext.GenerateSessionIdentifier(this);
			Context.Log(LogType.ConsumerStartup, source: this);
			Warehouse = new Warehouse();
			
			// init the session log
			Warehouse.Store(LOG_NAME, this, new[] { $"[{context.GetTimeNow()}] Log Starting" }, LoadingDockPolicies);
		}

		public override string ToString()
		{
			return "LogEventWriter";
		}
	}
}
