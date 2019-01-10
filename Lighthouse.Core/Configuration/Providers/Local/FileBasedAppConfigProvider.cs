using Lighthouse.Core.Configuration.Formats.YAML;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration.Providers.Local
{
	public class FileBasedAppConfigProvider : FileBasedConfigProvider<LighthouseYamlAppConfig>, IAppConfigurationProvider
	{
		private IList<IServiceRepository> ServiceRepositories { get; set; } = new List<IServiceRepository>();
		private IList<ServiceLaunchRequest> ServiceLaunchRequests { get; set; } = new List<ServiceLaunchRequest>();

		public int MaxThreadCount { get { return Config.MaxThreadCount; } }
		public string Version { get { return Config.Version; } }

		public FileBasedAppConfigProvider(ILighthouseServiceContainer lighthouseContainer, IFileContentProvider fileContentProvider)
			: base(lighthouseContainer, fileContentProvider)
		{
		}

		public FileBasedAppConfigProvider(ILighthouseServiceContainer lighthouseContainer, string configFilePath = null)
			: base(lighthouseContainer, configFilePath)
		{
		}

		public FileBasedAppConfigProvider(ILighthouseServiceContainer lighthouseContainer, string configFilePath = null, string version = "", int maxThreadCount = 1)
			: base(lighthouseContainer, configFilePath)
		{
			// this is all getting whacky, do we REALLY need to be able to completely control the in-memory representation of the config object
			Config = new LighthouseYamlAppConfig
			{
				Version = version,
				MaxThreadCount = maxThreadCount,
				ServiceRepositories = new List<LighthouseYamlConfigServiceRepositoryDescriptor>(),
				Services = new List<LighthouseYamlConfigServiceDescriptor>(),
				Name = "config",
				ConfigType = LighthouseConfigType.App.ToString()
			};
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
			Config.MaxThreadCount = config.MaxThreadCount;
			Config.Version = config.Version;

			foreach (var repo in config.ServiceRepositories)
			{
				ServiceRepositories.Add(repo.ToLocalRepository());
			}

			foreach (var service in config.Services)
			{
				ServiceLaunchRequests.Add(service.ToServiceLaunchRequest());
			}
		}

		public void AddServiceLaunchRequest(ServiceLaunchRequest launchRequest)
		{
			// add the request, and save the file
			// TODO: we probably need to time "save" a bit more clearly. 
			// TBH, I'm not a big fan of how the services are managed. 
			// Having a single file for configuration is very important, but updating it programmatically will add risk

			ServiceLaunchRequests.Add(launchRequest);
			Config.Services.Add(launchRequest.ToLighthouseYamlConfigServiceDescriptor());			
		}
	}
}
