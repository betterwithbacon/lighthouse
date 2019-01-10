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
		void Load();
		void Save();
	}

	public interface IAppConfigurationProvider :IConfigurationProvider
	{
		string Version { get; }
		int MaxThreadCount { get; }

		IEnumerable<IServiceRepository> GetServiceRepositories();
		IEnumerable<ServiceLaunchRequest> GetServiceLaunchRequests();
		void AddServiceLaunchRequest(ServiceLaunchRequest launchRequest);
	}		 
}
