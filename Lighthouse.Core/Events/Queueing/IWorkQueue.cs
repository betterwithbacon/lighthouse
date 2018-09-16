using BusDriver.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Events.Queueing
{
    public interface IWorkQueue<out T>
    {
		IEnumerable<T> Dequeue(int count);
		void Enqueue(IEvent ev);
    }
}
