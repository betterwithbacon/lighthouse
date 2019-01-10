using System;
using System.Collections.Generic;
using System.Linq;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Events;
using Lighthouse.Core.Logging;
using Lighthouse.Server.Management;

namespace Lighthouse.Server
{
	internal class ServiceInstallationEventConsumer : BaseEventConsumer
	{
		public override IList<Type> Consumes { get; } = new[] { typeof(ServiceInstallationEvent) };

		public void HandleEvent(ServiceInstallationEvent installationEvent)
		{
			var serviceName = installationEvent.ServiceName;

			// find the service to install
			if (string.IsNullOrEmpty(serviceName))
				throw new ArgumentNullException(nameof(installationEvent.ServiceName));

			// get the descriptor
			var serviceDescriptor = Container.FindServiceDescriptor(serviceName).FirstOrDefault();
			if (serviceDescriptor == null)
				throw new ApplicationException("Service not found");

			// modify the service registry to have this service to be available ion the container, 
			// currently that's just mean a launch request, so the next time the server starts up, it'll run
			Container.AddServiceLaunchRequest(new ServiceLaunchRequest(serviceDescriptor.Name), true, installationEvent.AutoStart);

			// install was successful
			var installedEvent = new ServiceInstalledEvent(serviceName, Container);
			Container.EmitEvent(installedEvent, this);
		}
	}
}