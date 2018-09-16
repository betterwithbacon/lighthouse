using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Events
{
	public interface IEvent
	{
		IEventContext Context { get; }
		DateTime Time { get; }
	}
}
