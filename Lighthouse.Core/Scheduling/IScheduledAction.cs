using BusDriver.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Scheduling
{
    public interface IScheduledAction
    {
		Schedule Schedule { get; }
		Action<EventContext> ActionToPerform { get; }
    }
}
