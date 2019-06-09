using System;

namespace Lighthouse.Core.Configuration.ServiceDiscovery
{
    public class ServiceLaunchRequest
    {
        public Type ServiceType { get; }
        public string ServiceName { get; set; }
        public ServiceLaunchRequestType LaunchType
        {
            get
            {
                if (ServiceType != null && ServiceName != null)
                    return ServiceLaunchRequestType.ByTypeThenName;
                else if (ServiceType != null)
                    return ServiceLaunchRequestType.ByType;
                else
                    // this is the most common scenario, if the name is null/empty, then consumers can throw errors.
                    return ServiceLaunchRequestType.ByServiceName;
            }
        }

        public ServiceLaunchRequest(Type type)
        {
            ServiceType = type;
        }

        public ServiceLaunchRequest(string serviceName)
        {
            ServiceName = serviceName;
        }
    }

    public static class LighthouseServiceDescriptorExtensions
    {
        public static ServiceLaunchRequest ToServiceLaunchRequest(this ILighthouseServiceDescriptor descriptor) 
            => new ServiceLaunchRequest(descriptor.ServiceType);
    }

	/// <summary>
	/// Defines how the launch request should be serviced.	
	/// </summary>
	public enum ServiceLaunchRequestType
	{
		ByTypeThenName,
		ByType,		
		ByServiceName		
	}
}