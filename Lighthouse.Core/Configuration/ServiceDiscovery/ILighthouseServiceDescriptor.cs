using System;

namespace Lighthouse.Core.Configuration.ServiceDiscovery
{
	public interface ILighthouseServiceDescriptor
	{
		string Name { get; }
        Version Version { get; }
        Type ServiceType { get; }

        [Obsolete]
        string Type { get; }

        [Obsolete]
        string Alias { get; }
	}
}