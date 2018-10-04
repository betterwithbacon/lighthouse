using System.Collections.Generic;

namespace Lighthouse.Core.Configuration.ServiceDiscovery
{
	public interface IServiceRepository : ILighthouseComponent
	{
		IEnumerable<ILighthouseServiceDescriptor> GetServiceDescriptors();
	}
}