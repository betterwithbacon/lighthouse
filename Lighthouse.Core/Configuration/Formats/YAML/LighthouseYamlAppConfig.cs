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
		public List<MemoryServiceDescriptor> Services { get; set; }

		//[YamlMember(Alias ="serviceRepositories")]
		public List<LighthouseYamlConfigServiceRepositoryDescriptor> ServiceRepositories { get; set; }
	}
}
