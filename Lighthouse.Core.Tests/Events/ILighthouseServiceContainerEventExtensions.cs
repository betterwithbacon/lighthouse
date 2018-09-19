using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using FluentAssertions;
using Xunit.Abstractions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Lighthouse.Server;
using Lighthouse.Core.Events.Logging;
using Lighthouse.Core.Scheduling;
using Lighthouse.Core.Events.Time;
using Lighthouse.Core.Events.Queueing;
using Lighthouse.Core.Events;

namespace Lighthouse.Core.Tests.Events
{    
	public static class ILighthouseServiceContainerEventExtensions
	{
		public static void AssertEventExists<TEvent>(this ILighthouseServiceContainer container, int count = 1, Func<IEvent, bool> additionalFilter = null, int? atLeast = null)
			where TEvent : IEvent
		{
			var recCount = container.GetAllReceivedEvents().Where(e => e.GetType() == typeof(TEvent) && (additionalFilter?.Invoke(e) ?? true)).Count();

			if (atLeast.HasValue)
				recCount.Should().BeGreaterOrEqualTo(atLeast.Value);
			else
				recCount.Should().Be(count);
		}
	}
}
