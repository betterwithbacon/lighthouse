using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration.Providers
{
	public interface IConfigurationProvider : IResourceProvider
	{
		string Name { get; }
		LighthouseConfigType ConfigType { get; }
	}

	public interface IAppConfigurationProvider :IConfigurationProvider
	{
		IEnumerable<IServiceRepository> GetServiceRepositories();
		IEnumerable<ServiceLaunchRequest> GetServiceLaunchRequests();
	}		 
}
