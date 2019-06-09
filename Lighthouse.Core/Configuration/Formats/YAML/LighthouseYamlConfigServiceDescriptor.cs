﻿using Lighthouse.Core.Configuration.ServiceDiscovery;
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

        public Version Version { get; set; }

        public Type ServiceType { get; set; }
    }

	public static class LighthouseYamlConfigServiceDescriptorExtensions
	{
		public static ServiceLaunchRequest ToServiceLaunchRequest(this LighthouseYamlConfigServiceDescriptor repo)
		{
			return new ServiceLaunchRequest(repo.Name);	
		}

		public static LighthouseYamlConfigServiceDescriptor ToLighthouseYamlConfigServiceDescriptor(this ServiceLaunchRequest repo)
		{
			return new LighthouseYamlConfigServiceDescriptor
			{
				Name = repo.ServiceName,
				Type = repo.ServiceType?.AssemblyQualifiedName,
				Alias = repo.ServiceName
			};
		}
	}
}
