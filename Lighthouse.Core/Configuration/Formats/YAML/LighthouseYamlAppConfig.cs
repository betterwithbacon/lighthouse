using Lighthouse.Core.Configuration.Formats.Memory;
using Lighthouse.Core.Configuration.Providers.Local;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Lighthouse.Core.Configuration.Formats.YAML
{
	public class LighthouseYamlAppConfig : LighthouseYamlBaseConfig
	{		
		public int MaxThreadCount { get; set; }
		public List<LighthouseYamlConfigServiceDescriptor> Services { get; set; }

		//[YamlMember(Alias ="serviceRepositories")]
		public List<LighthouseYamlConfigServiceRepositoryDescriptor> ServiceRepositories { get; set; }
	}

	public static class LighthouseYamlAppConfigExtensions
	{
		public static void LoadConfigurationAppYaml(this ILighthouseServiceContainer lighthouseServiceContainer, string yamlContent)
		{
			lighthouseServiceContainer.RegisterResourceProvider(new FileBasedAppConfigProvider(lighthouseServiceContainer, new MemoryContentProvider(yamlContent)));
		}
	}
		
}
