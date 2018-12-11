using Lighthouse.Core.Configuration.Formats.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Lighthouse.Core.Configuration.ServiceDiscovery.Local
{
	// a repository which returns all services within the application domain
	public class LocalServiceRepository : IServiceRepository
	{
		readonly ConcurrentBag<ILighthouseServiceDescriptor> ServiceDescriptors = new ConcurrentBag<ILighthouseServiceDescriptor>();
		
		public ILighthouseServiceContainer Container { get; }

		public LocalServiceRepository(ILighthouseServiceContainer serviceContainer)
		{
			Container = serviceContainer;
			LoadServices();
		}

		private void LoadServices()
		{
			// TODO: go to the container, and get the services?
			foreach(var type in Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(t => t.IsAssignableFrom(typeof(ILighthouseService))))
			{
				ServiceDescriptors.Add(type.ToServiceDescriptor());
			}
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
		public ILighthouseServiceContainer Container { get; }

		public MemoryServiceRepository(ILighthouseServiceContainer serviceContainer)
		{
			Container = serviceContainer;
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
		public ILighthouseServiceContainer Container { get; }

		public RemoteServiceRepository(ILighthouseServiceContainer serviceContainer, string uri)
		{
			URI = uri;
			Container = serviceContainer;
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
