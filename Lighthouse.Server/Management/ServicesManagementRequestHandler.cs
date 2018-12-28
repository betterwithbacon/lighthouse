using Lighthouse.Core.Hosting;
using Lighthouse.Core.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lighthouse.Server.Management
{
	public class ServicesManagementRequestHandler : IManagementRequestHandler
	{
		public object Handle(string rawRequestPayload, IManagementRequestContext requestContext)
		{
			var servicesRequest = rawRequestPayload.DeserializeForManagementInterface<LighthouseServerRequest<ListServicesRequest>>();
			// TODO: at some point, all of this type name matching, needs to be delegated to "Service Discovery" where these things can be found by names,hashes, etc.
			var typeNameToFilterOn = servicesRequest.Request.ServiceDescriptorToFind.Type;
			return new LighthouseServerResponse<List<LighthouseServiceRemotingWrapper>>(
				requestContext.Container.GetStatus(),
				requestContext.Container.GetRunningServices((serviceRun) => serviceRun.Service.GetType().AssemblyQualifiedName == typeNameToFilterOn)
					.Select(lsr => new LighthouseServiceRemotingWrapper(lsr.ID, lsr.Service))
					.ToList()
			);
		}
	}
}
