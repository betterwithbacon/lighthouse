using Lighthouse.Core.Utils;
using System;

namespace Lighthouse.Core.Events.Logging
{
	public class LogEvent : BaseEvent
	{
		public string Message { get; set; }
		public ILighthouseComponent Source { get; }

		public LogEvent(ILighthouseServiceContainer container, ILighthouseComponent source)
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