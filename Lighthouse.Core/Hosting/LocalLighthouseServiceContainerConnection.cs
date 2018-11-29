﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lighthouse.Core.Management;

namespace Lighthouse.Core.Hosting
{
	public class LocalLighthouseServiceContainerConnection : ILighthouseServiceContainerConnection
	{
		public bool IsConnected => RemoteContainer != null;

		public ILighthouseServiceContainer RemoteContainer { get; }

		public IList<LighthouseServiceContainerConnectionStatus> ConnectionHistory { get; } =
			new List<LighthouseServiceContainerConnectionStatus>();

		public bool IsBidirectional { get; }

		public ILighthouseServiceContainer Container { get; }

		public LocalLighthouseServiceContainerConnection(ILighthouseServiceContainer localContainer, ILighthouseServiceContainer remoteContainer, bool isBidirectional = true)
		{
			Container = localContainer;
			RemoteContainer = remoteContainer;
			IsBidirectional = isBidirectional;
		}

		public Task<bool> TryConnect()
		{
			// it's always connected
			return Task.FromResult(true);
		}

		public override string ToString()
		{
			return $" Local Service Container '{RemoteContainer?.ServerName}'";
		}

		public Task<IEnumerable<LighthouseServiceProxy<T>>> FindServices<T>()
			where T : class, ILighthouseService
		{
			// this container is local, so we're sharing the same memory pool,
			return Task.FromResult(
				RemoteContainer
					.FindServices<T>()
					.OfType<T>()
					.Select(s => new LighthouseServiceProxy<T>(this, s))
				);
		}

		public Task<ManagementInterfaceResponse> SubmitManagementRequest(IManagementRequest managementRequest)
		{
			throw new System.NotImplementedException();
		}
	}
}
