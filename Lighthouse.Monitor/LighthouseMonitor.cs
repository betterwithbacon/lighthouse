﻿using Lighthouse.Core;
using Lighthouse.Core.Configuration;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;

namespace Lighthouse.Monitor
{
	public class LighthouseMonitor : ILighthouseComponent
	{
		public ILighthouseServiceContainer LighthouseContainer => throw new NotImplementedException();

		public event StatusUpdatedEventHandler StatusUpdated;

		public void RegisterServiceRequest(ServiceLaunchRequest request)
		{
			
		}
	}
}