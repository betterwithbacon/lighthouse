using Lighthouse.Core;
using Lighthouse.Core.Events;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Management;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Server.Management
{
	public class ServerManagementRequestHandler : IManagementRequestHandler
	{
		public async Task<object> Handle(string request, IManagementRequestContext requestContext)
		{
			var requestDetails = request.DeserializeForManagementInterface<ServerManagementRequest>();

			switch (requestDetails.RequestType)
			{
				case ServerManagementRequestType.Install:
					// create an event listener for an installation succeeded.
					//requestContext.Container.RegisterEventConsumer<ServiceInstalledEvent>(
					//	new OneTimeHandler<ServiceInstalledEvent>( (ev) => 
					//);

					//if (requestDetails.RequestParameters.ContainsKey(ServerManagementRequest.RequestTypes.Install.Arguments.ServiceName))					
					if (requestDetails.RequestParameters.TryGetValue(ServerManagementRequest.RequestTypes.Install.Arguments.ServiceName, out var serviceNameRaw))
					{
						if(!(serviceNameRaw is string serviceName))
							throw new ApplicationException($"Invalid service name was specified. {serviceNameRaw}");

						// emit an event to install this service in this container
						await requestContext.Container.EmitEvent(
							new ServiceInstallationEvent(serviceName, requestContext.Container)
						);						
					}
					else
					{
						if (!(serviceNameRaw is string serviceName))
							throw new ApplicationException("No service name was specified.");
					}
					break;
			}

			return $"success";
		}
	}

	public class ServiceInstalledEvent : BaseEvent
	{
		public string ServiceInstalled { get; }

		public ServiceInstalledEvent(string serviceInstalled, ILighthouseServiceContainer container)
			: base(container)
		{
			ServiceInstalled = serviceInstalled;
		}
	}

	// TODO: it does occur to me, that theoretically, it'd be cool if at some point in the future,
	// The CLI to manage a container cluster, literally, could just emit events directly into the container, 
	// and not have to worry about the asbstractions between the CLI and the server, but A LONG way from that
	public class ServiceInstallationEvent : BaseEvent
	{
		public string ServiceName { get; }
		public bool AutoStart { get; }

		public ServiceInstallationEvent(string serviceName, ILighthouseServiceContainer container, bool autoStart = false)
			: base(container)
		{
			ServiceName = serviceName;
			AutoStart = autoStart;
		}
	}
}