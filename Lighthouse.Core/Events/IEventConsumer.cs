using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lighthouse.Core.Events
{
	public interface IEventConsumer : IEventActionTrigger, ILighthouseLogSource, ILighthouseComponent
	{
		IList<Type> Consumes { get; }

		void HandleEvent(EventWrapper<IEvent> ev);

		void Init(ILighthouseServiceContainer container);
	}

	//public static class IEventExtensions
	//{

	//}
}
