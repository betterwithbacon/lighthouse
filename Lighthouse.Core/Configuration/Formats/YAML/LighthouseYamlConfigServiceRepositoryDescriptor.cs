using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Configuration.ServiceDiscovery.Local;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration.Formats.YAML
{
	public class LighthouseYamlConfigServiceRepositoryDescriptor
	{
		public string Name { get; set; }
		public string Uri { get; set; }
	}

	public static class LighthouseYamlConfigServiceRepositoryDescriptorExtensions
	{
		public static IServiceRepository ToLocalRepository(this LighthouseYamlConfigServiceRepositoryDescriptor repoDescriptor)
		{
			//var repo = new RemoteServiceRepository(null,);

			//repoDescriptor.
			//repo.AddServiceDescriptor(
			//	new 


			//);

			return null;
		}
	}

	
}
