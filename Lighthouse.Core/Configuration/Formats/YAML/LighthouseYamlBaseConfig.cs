using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Lighthouse.Core.Configuration.Formats.YAML
{
	public class LighthouseYamlBaseConfig
	{
		public static readonly string DEFAULT_CONFIG_FILENAME = "lighthouse.yaml";
		public string Name { get; set; }
		public string Version { get; set; }
		[YamlMember(Alias = "type")]
		public string ConfigType { get; set; }
	}
}
