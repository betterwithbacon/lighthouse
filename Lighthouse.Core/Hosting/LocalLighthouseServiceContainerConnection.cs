using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lighthouse.Core.Hosting
{
	public class LocalLighthouseServiceContainerConnection : ILighthouseServiceContainerConnection
	{
		public bool IsConnected => RemoteContainer != null;

		public ILighthouseServiceContainer RemoteContainer { get; }

		public IList<LighthouseServiceContainerConnectionStatus> ConnectionHistory { get; } =
			new List<LighthouseServiceContainerConnectionStatus>();

		public bool IsBidirectional { get; }

		public ILighthouseServiceContainer LighthouseContainer { get; }

		public LocalLighthouseServiceContainerConnection(ILighthouseServiceContainer localContainer, ILighthouseServiceContainer remoteContainer, bool isBidirectional = true)
		{
			LighthouseContainer = localContainer;
			RemoteContainer = remoteContainer;
			IsBidirectional = isBidirectional;
		}

		public bool TryConnect()
		{
			// it's always connected
			return true;
		}

		public override string ToString()
		{
			return $"Local Service Container '{RemoteContainer?.ServerName}'";
		}

		public IEnumerable<LighthouseServiceProxy<T>> FindServices<T>()
			where T : class, ILighthouseService
		{
			// this container is local, so we're sharing the same memory pool,
			return RemoteContainer.FindServices<T>().OfType<T>().Select(s => new LighthouseServiceProxy<T>(this, s));
		}
	}
}
