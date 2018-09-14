using Lighthouse.Core.Configuration.Formats.YAML;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration.Providers.Local
{
	public class FileBasedAppConfigProvider : FileBasedConfigProvider<LighthouseYamlAppConfig>
	{
		private IList<IServiceRepository> ServiceRepositories { get; set; } = new List<IServiceRepository>();
		private IList<ServiceLaunchRequest> ServiceLaunchRequests { get; set; } = new List<ServiceLaunchRequest>();


		public int MaxThreadCount { get; private set; }
		public string Version { get; private set; }

		public FileBasedAppConfigProvider(ILighthouseServiceContainer lighthouseContainer, string configFilePath = null)
			: base(lighthouseContainer, configFilePath)
		{
		}

		public IEnumerable<IServiceRepository> GetServiceRepositories()
		{
			return ServiceRepositories;
		}

		public IEnumerable<ServiceLaunchRequest> GetServiceLaunchRequests()
		{
			return ServiceLaunchRequests;
		}

		protected override void LoadTypeSpecificConfig(LighthouseYamlAppConfig config)
		{
			MaxThreadCount = config.MaxThreadCount;
			Version = config.Version;

			foreach (var repo in config.ServiceRepositories)
			{
				ServiceRepositories.Add(repo.ToLocalRepository());
			}

			foreach (var service in config.Services)
			{
				ServiceLaunchRequests.Add(service.ToServiceLaunchRequest());
			}
		}
	}
}
