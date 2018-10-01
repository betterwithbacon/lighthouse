using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration.Formats.YAML
{
	public class LighthouseYamlConfigServiceDescriptor : ILighthouseServiceDescriptor
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string Alias { get; set; }
	}

	public static class LighthouseYamlConfigServiceDescriptorExtensions
	{
		public static ServiceLaunchRequest ToServiceLaunchRequest(this LighthouseYamlConfigServiceDescriptor repo)
		{
			return null;
		}
	}
}
