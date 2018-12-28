using Lighthouse.Core.Management;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Server.Management
{
	public class PingManagementRequestHandler : IManagementRequestHandler
	{
		public object Handle(string rawRequestPayload, IManagementRequestContext requestContext)
		{
			// return the status of the requesting context
			return requestContext.Container.GetStatus();
		}
	}
}
