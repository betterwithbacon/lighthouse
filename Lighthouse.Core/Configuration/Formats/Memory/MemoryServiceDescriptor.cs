using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration.Formats.Memory
{
	public class ServiceDescriptor : ILighthouseServiceDescriptor
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string Alias { get; set; }
	}

	public static class MemoryServiceDescriptorExtensions
	{
		public static ServiceLaunchRequest ToServiceLaunchRequest(this ServiceDescriptor repo)
		{
			if(repo.Name == null)
				return new ServiceLaunchRequest(repo.Type);
			else
				return new ServiceLaunchRequest(repo.Name);
		}
	}
}
