using Lighthouse.Core.Management;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Server.Management
{
	public class PingManagementRequestHandler : IManagementRequestHandler
	{
		public string Handle(string rawRequestPayload, IManagementRequestContext requestContext)
		{
			// return the status of the requesting context
			return requestContext.GetStatus();
		}
	}
}
