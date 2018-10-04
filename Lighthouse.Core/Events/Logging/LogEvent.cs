using Lighthouse.Core.Utils;
using System;

namespace Lighthouse.Core.Events.Logging
{
	public class LogEvent : IEvent
	{
		public string Message { get; set; }
		public DateTime Time { get; set; }
		public ILighthouseServiceContainer LighthouseContainer { get; }
		public ILighthouseComponent Source { get; }

		public LogEvent(ILighthouseServiceContainer container, ILighthouseComponent source)
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