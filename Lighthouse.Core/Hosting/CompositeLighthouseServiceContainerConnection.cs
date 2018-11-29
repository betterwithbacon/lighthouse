using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lighthouse.Core.Management;

namespace Lighthouse.Core.Hosting
{
	public class CompositeLighthouseServiceContainerConnection : ILighthouseServiceContainerConnection
	{
		public bool IsConnected => isConnectedFunc?.Invoke(this) ?? true;
		private Func<CompositeLighthouseServiceContainerConnection, bool> isConnectedFunc;

		public bool IsBidirectional => throw new NotImplementedException();
		private Func<CompositeLighthouseServiceContainerConnection, bool> isBidirectionalFunc;

		public IList<LighthouseServiceContainerConnectionStatus> ConnectionHistory { get; set; }
			= new List<LighthouseServiceContainerConnectionStatus>();

		public ILighthouseServiceContainer Container { get; private set; }

		public CompositeLighthouseServiceContainerConnection(
			Func<CompositeLighthouseServiceContainerConnection, bool>  isConnectedFunc,
			Func<CompositeLighthouseServiceContainerConnection, bool> isBidirectionalFunc,
			ILighthouseServiceContainer container)
		{
			this.isConnectedFunc = isConnectedFunc;
			this.isBidirectionalFunc = isBidirectionalFunc;
			this.Container = container;
		}

		public Task<bool> TryConnect()
		{
			return Task.FromResult(IsConnected);
		}

		Task<IEnumerable<LighthouseServiceProxy<T>>> ILighthouseServiceContainerConnection.FindServices<T>()
		{
			throw new NotImplementedException();
		}

		public Task<ManagementInterfaceResponse> SubmitManagementRequest(IManagementRequest managementRequest)
		{
			throw new NotImplementedException();
		}
	}
}
