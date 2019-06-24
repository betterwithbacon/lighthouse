using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Lighthouse.Core.Configuration.ServiceDiscovery;

namespace Lighthouse.Core.Configuration.Providers.Local
{
	/// <summary>
	/// Represents an in-memory, non-persisted configuration provider 
	/// </summary>
	//public class MemoryAppConfigurationProvider : IAppConfigurationProvider
	//{
	//	public string Name { get; }

	//	public LighthouseConfigType ConfigType { get; }

	//	public string Version { get; }

	//	public int MaxThreadCount { get; }

	//	private readonly ConcurrentBag<ServiceLaunchRequest> ServiceLaunchRequests;
	//	private readonly ConcurrentBag<IServiceRepository> ServiceRepositories;

	//	public MemoryAppConfigurationProvider(string appName, string version, int maxThreadCount)
	//	{	
	//		Name = appName;
	//		ConfigType = LighthouseConfigType.App;
	//		Version = version;
	//		MaxThreadCount = maxThreadCount;

	//		ServiceLaunchRequests = new ConcurrentBag<ServiceLaunchRequest>();
	//		ServiceRepositories = new ConcurrentBag<IServiceRepository>();
	//	}

	//	public IEnumerable<ServiceLaunchRequest> GetServiceLaunchRequests()
	//	{
	//		return ServiceLaunchRequests;
	//	}

	//	public IEnumerable<IServiceRepository> GetServiceRepositories()
	//	{
	//		return ServiceRepositories;
	//	}

	//	public void AddServiceLaunchRequest(ServiceLaunchRequest request)
	//	{
	//		ServiceLaunchRequests.Add(request);
	//	}

	//	public void AddServiceRepository(IServiceRepository repository)
	//	{
	//		ServiceRepositories.Add(repository);
	//	}

	//	public override string ToString()
	//	{
	//		return "In-Memory AppConfig";
	//	}

	//	public void Load()
	//	{
	//		// it's always ready ;)
	//	}

	//	public void Save()
	//	{
	//		// not much to do here really
	//	}
	//}
}
