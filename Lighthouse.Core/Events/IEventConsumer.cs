using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lighthouse.Core.Events
{
	public interface IEventConsumer
	{
	}

	public interface IEventConsumer<TEvent> : IEventConsumer
		where TEvent : IEvent
	{
		void HandleEvent(TEvent e);
	}
}
