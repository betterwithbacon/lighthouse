using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Configuration.ServiceDiscovery
{
	public class LighthouseComponentDescriptor<T>
		where T : ILighthouseComponent
	{
		public bool CanBuild()
		{
			// TODO: fix this
			return true;
		}

		public T Build()
		{
			return default;
		}
	}
}
