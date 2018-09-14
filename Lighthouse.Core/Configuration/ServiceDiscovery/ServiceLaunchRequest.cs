using System;

namespace Lighthouse.Core.Configuration.ServiceDiscovery
{
	public class ServiceLaunchRequest
	{
		public Type ServiceType { get; }		
		public string Name { get; set; }

		public ServiceLaunchRequest(Type type)
		{
			ServiceType = type;
		}

		public ServiceLaunchRequest(string typeName)
		{
			ServiceType = Type.GetType(typeName);
		}
	}
}