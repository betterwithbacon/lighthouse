using System;

namespace Lighthouse.Core.Events.Logging
{
	public class LogEvent : IEvent
	{
		public string Message { get; set; }
		public DateTime Time { get; set; }
		public ILighthouseServiceContainer LighthouseContainer { get; }
		public ILighthouseComponent Source { get; }

		public LogEvent(ILighthouseComponent source)
		{
			Source = source;
			LighthouseContainer = source.LighthouseContainer;
		}

		public override string ToString()
		{
			return $"LogEvent: {Message.Substring(0,25)}";
		}
	}
}