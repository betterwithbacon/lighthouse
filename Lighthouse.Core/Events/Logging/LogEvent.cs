using Lighthouse.Core.Utils;
using System;

namespace Lighthouse.Core.Events.Logging
{
	public class LogEvent : BaseEvent
	{
		public string Message { get; set; }
		public object Source { get; }

		public LogEvent(ILighthouseServiceContainer container, object source)
			: base(container)
		{
			Source = source;
			LighthouseContainer = container;
		}

		public override string ToString()
		{
			return $"LogEvent: {Message?.ToLogSummary(100) ?? "<no message>"}";
		}
	}
}