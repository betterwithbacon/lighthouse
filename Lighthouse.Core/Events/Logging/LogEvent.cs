using System;

namespace Lighthouse.Core.Events.Logging
{
	public class LogEvent : IEvent
	{
		public string Message { get; set; }

		public DateTime Time { get; set; }

		public IEventContext Context { get; private set; }

		public LogEvent(IEventContext context)
		{
			Context = context;
		}

		public override string ToString()
		{
			return $"LogEvent: {Message.Substring(0,25)}";
		}
	}
}