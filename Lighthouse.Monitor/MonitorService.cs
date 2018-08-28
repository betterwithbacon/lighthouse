using Lighthouse.Core;
using System;

namespace Lighthouse.Monitor
{
	public class MonitorService : ILighthouseComponent
	{
		public ILighthouseServiceContainer LighthouseContainer => throw new NotImplementedException();

		public event StatusUpdatedEventHandler StatusUpdated;
	}
}
