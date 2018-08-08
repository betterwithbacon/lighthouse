using Lighthouse.Core;
using System;

namespace Lighthouse.Monitor
{
	public class MonitorService : ILighthouseComponent
	{
		public event StatusUpdatedEventHandler StatusUpdated;
	}
}
