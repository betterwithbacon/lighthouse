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
		
		public ILighthouseServiceContainer LighthouseContainer { get; }

		public LocalServiceRepository(ILighthouseServiceContainer serviceContainer)
		{
			LighthouseContainer = serviceContainer;
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
		public ILighthouseServiceContainer LighthouseContainer { get; }

		public MemoryServiceRepository(ILighthouseServiceContainer serviceContainer)
		{
			LighthouseContainer = serviceContainer;
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

	public class RemoteServiceRepository : IServiceRepository
	{
		readonly ConcurrentBag<ILighthouseServiceDescriptor> ServiceDescriptors = new ConcurrentBag<ILighthouseServiceDescriptor>();
		private readonly string URI;
		public ILighthouseServiceContainer LighthouseContainer { get; }

		public RemoteServiceRepository(ILighthouseServiceContainer serviceContainer, string uri)
		{
			URI = uri;
			LighthouseContainer = serviceContainer;
			RetrieveServiceDescriptors();
		}

		private void RetrieveServiceDescriptors()
		{
			//var remoteRepos = LighthouseContainer.RetrieveRemoteComponent<IServiceRepository>(URI);

		}

		public IEnumerable<ILighthouseServiceDescriptor> GetServiceDescriptors()
		{
			return ServiceDescriptors;
		}
	}
}
