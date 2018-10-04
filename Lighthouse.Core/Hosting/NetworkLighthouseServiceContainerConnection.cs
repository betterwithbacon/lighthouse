using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Lighthouse.Core.Hosting
{
	public class NetworkLighthouseServiceContainerConnection : ILighthouseServiceContainerConnection
	{
		public IPAddress IpAddress { get; private set; }

		public bool IsConnected { get; private set; } = false;

		public ILighthouseServiceContainer LighthouseServiceContainer { get; private set; }

		public IList<LighthouseServiceContainerConnectionStatus> ConnectionHistory { get; } =
			new List<LighthouseServiceContainerConnectionStatus>();

		public bool IsBidrectional => false; // for now, this will NOT provide bidirectional communication, because all sorts of networking implications will be in place

		public bool TryConnect()
		{
			IsConnected = false;
			return true;
		}
	}
}
