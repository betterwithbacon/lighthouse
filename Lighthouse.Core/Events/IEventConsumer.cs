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

		void HandleEvent(IEvent ev);

		void Init(ILighthouseServiceContainer container);
	}

	public static class IEventExtensions
	{
		public static void ThrowIfInvalidEvent(this IEventConsumer consumer, IEvent ev)
		{
			if (!consumer.Consumes.Contains(ev?.GetType()))
				throw new InvalidEventException(ev.GetType(), consumer.Consumes.ToArray());
		}
	}
}
