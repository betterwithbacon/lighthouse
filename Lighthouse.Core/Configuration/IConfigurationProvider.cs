using Lighthouse.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration
{
	public interface IConfigurationProvider : IResourceProvider
	{
		IEnumerable<IServiceRepository> GetServiceRepositories();
		IEnumerable<ServiceLaunchRequest> GetServiceLaunchRequests();
	}
}
