using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Hosting
{
	public class LocalLighthouseServiceContainerConnection : ILighthouseServiceContainerConnection
	{
		public bool IsConnected => LighthouseServiceContainer != null;

		public ILighthouseServiceContainer LighthouseServiceContainer { get; }
		
		public IList<LighthouseServiceContainerConnectionStatus> ConnectionHistory { get; } =
			new List<LighthouseServiceContainerConnectionStatus>();

		public bool IsBidrectional { get; }

		public LocalLighthouseServiceContainerConnection(ILighthouseServiceContainer lighthouseServiceContainer, bool isBidirectional = true)
		{
			LighthouseServiceContainer = lighthouseServiceContainer;
			IsBidrectional = isBidirectional;
		}

		public bool TryConnect()
		{
			// it's always connected
			return true;
		}

		public override string ToString()
		{
			return $"Local Service Container '{LighthouseServiceContainer?.ServerName}'";
		}
	}
}
