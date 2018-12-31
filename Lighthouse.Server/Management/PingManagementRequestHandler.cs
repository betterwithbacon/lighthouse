using Lighthouse.Core.Management;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Server.Management
{
	public class PingManagementRequestHandler : IManagementRequestHandler
	{
		public async Task<object> Handle(string rawRequestPayload, IManagementRequestContext requestContext)
		{
			// return the status of the requesting context
			return await Task.FromResult(requestContext.Container.GetStatus());
		}
	}
}
