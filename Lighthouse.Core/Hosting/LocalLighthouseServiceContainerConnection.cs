using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			return $"Local Container '{Container?.ServerName}', Remote Container: '{RemoteContainer?.ServerName}'";
		}

        public TResponse MakeRequest<TRequest, TResponse>(TRequest storageRequest)
            where TRequest : class
        {
            return RemoteContainer.HandleRequest<TRequest, TResponse>(storageRequest);
        }
    }
}
