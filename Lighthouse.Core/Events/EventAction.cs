﻿using System;

namespace Lighthouse.Core.Events
{
	public class EventAction
	{
		public Action<IEvent> Action { get; set; }

		public void Run(IEventActionTrigger trigger, IEvent ev)
		{
			Action?.Invoke(ev);
		}
	}
}