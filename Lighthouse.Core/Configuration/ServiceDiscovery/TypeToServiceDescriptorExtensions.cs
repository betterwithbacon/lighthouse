using Lighthouse.Core.Configuration.Formats.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration.ServiceDiscovery
{
	public static class TypeToServiceDescriptorExtensions
	{
		public static ILighthouseServiceDescriptor ToServiceDescriptor(this Type type)
		{
			return new ServiceDescriptor
			{
				Name = type.Name,
				Type = type.AssemblyQualifiedName,
				Alias = type.Name
			};
		}
	}
}
