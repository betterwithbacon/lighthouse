using Lighthouse.Core.Configuration.Formats.YAML;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration.Providers.Local
{
	public class FileBasedAppConfigProvider : FileBasedConfigProvider<LighthouseYamlAppConfig> //, IAppConfigurationProvider
	{
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
				//ServiceRepositories = new List<LighthouseYamlConfigServiceRepositoryDescriptor>(),
				//Services = new List<LighthouseYamlConfigServiceDescriptor>(),
				Name = "config",
				ConfigType = LighthouseConfigType.App.ToString()
			};
		}

		protected override void LoadTypeSpecificConfig(LighthouseYamlAppConfig config)
		{
			Config.MaxThreadCount = config.MaxThreadCount;
			Config.Version = config.Version;
		}
	}
}
