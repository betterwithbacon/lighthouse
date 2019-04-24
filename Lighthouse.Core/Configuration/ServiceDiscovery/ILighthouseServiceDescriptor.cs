using System;

namespace Lighthouse.Core.Configuration.ServiceDiscovery
{
	public interface ILighthouseServiceDescriptor
	{
		string Name { get; }
        Version Version { get; }

        [Obsolete]
        string Type { get; }

        [Obsolete]
        string Alias { get; }
	}
}