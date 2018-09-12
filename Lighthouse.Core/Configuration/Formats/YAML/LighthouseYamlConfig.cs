using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Lighthouse.Core.Configuration.Formats.YAML
{
	public class LighthouseYamlConfig
	{
		public string Name { get; set; }
		public string Version { get; set; }
		public int MaxThreadCount { get; set; }
		public List<LighthouseYamlConfigServiceDescriptor> Services { get; set; }

		[YamlMember(Alias = "service-repositories")]
		public List<LighthouseYamlConfigServiceRepositoryDescriptor> ServiceRepositories { get; set; }
	}

	public class LighthouseYamlConfigServiceDescriptor
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string Alias { get; set; }
	}

	public class LighthouseYamlConfigServiceRepositoryDescriptor
	{
		public string Name { get; set; }
		public string Uri { get; set; }
	}
}
