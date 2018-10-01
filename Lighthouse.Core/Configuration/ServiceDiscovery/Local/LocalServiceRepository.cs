using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration.ServiceDiscovery.Local
{
	// a repository which returns all services within the application domain
	public class LocalServiceRepository : IServiceRepository
	{
		readonly ConcurrentBag<ILighthouseServiceDescriptor> ServiceDescriptors = new ConcurrentBag<ILighthouseServiceDescriptor>();
		public LocalServiceRepository()
		{
			LoadServices();
		}

		private void LoadServices()
		{
			
		}

		public IEnumerable<ILighthouseServiceDescriptor> GetServiceDescriptors()
		{
			return ServiceDescriptors;
		}
	}

	// a repository which returns all services within memory
	public class MemoryServiceRepository : IServiceRepository
	{
		readonly ConcurrentBag<ILighthouseServiceDescriptor> ServiceDescriptors = new ConcurrentBag<ILighthouseServiceDescriptor>();

		public MemoryServiceRepository()
		{
			
		}

		public void AddServiceDescriptor(ILighthouseServiceDescriptor serviceDescriptor)
		{
			ServiceDescriptors.Add(serviceDescriptor);
		}

		public IEnumerable<ILighthouseServiceDescriptor> GetServiceDescriptors()
		{
			return ServiceDescriptors;
		}
	}
}
