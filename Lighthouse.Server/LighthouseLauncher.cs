using Lighthouse.Core;

using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Server
{
	public class LighthouseLauncher<T>
		where T : ILighthouseServiceContainer
	{
		public T LighthouseContainer { get; private set; }

		private LighthouseLauncher()
		{
			LighthouseContainer = Activator.CreateInstance<T>();
		}

		public static LighthouseLauncher<TService> Create<TService>(string serviceName = null)
			where TService : ILighthouseServiceContainer
		{
			return new LighthouseLauncher<TService>();
		}

		public void AddLocalLogger()
		{

		}

		public T Build()
		{
			return LighthouseContainer;
		}
	}
}
