using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Lighthouse.Core.Storage;
using Lighthouse.Core.Utils;

namespace Lighthouse.Core.Events.Logging
{
    public class LogEventConsumer : BaseEventConsumer, IEventConsumer<LogEvent>
    {	
        public List<string> AllLogRecords => new List<string>();

		public void HandleEvent(LogEvent logEvent)
		{
			Container.Warehouse.Store(
                $"log_event_consume_{GetLongTimeName()}",
                (new[] { $"[{logEvent.EventTime}] {logEvent.Message}" }).ConvertToJson()
            );
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
