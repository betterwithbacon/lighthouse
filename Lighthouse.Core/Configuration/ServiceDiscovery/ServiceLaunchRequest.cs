using System;

namespace Lighthouse.Core.Configuration.ServiceDiscovery
{
    public class ServiceLaunchRequest
    {
        public Type ServiceType { get; }
        public string ServiceName { get; set; }

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
}