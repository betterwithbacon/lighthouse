using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Lighthouse.Core.Hosting
{
	public class NetworkLighthouseServiceContainerConnection : ILighthouseServiceContainerConnection
	{	
		public bool IsConnected { get; private set; } = false;

		public ILighthouseServiceContainer RemoteContainer
			=> throw new Exception("Can't retrieve direct container on a unidirectional connection. Use FindServices to retrieve components from the remote container.");
		
		public IPAddress RemoteServerAddress { get;}

		public IList<LighthouseServiceContainerConnectionStatus> ConnectionHistory { get; } =
			new List<LighthouseServiceContainerConnectionStatus>();

		public NetworkLighthouseServiceContainerConnection(ILighthouseServiceContainer localContainer, IPAddress ipAddress)
		{
			RemoteServerAddress = ipAddress;
			localContainer.GetNetworkProviders().First();
		}

		public bool IsBidirectional => false; // for now, this will NOT provide bidirectional communication, because all sorts of networking implications will be in place

		public ILighthouseServiceContainer LighthouseContainer => throw new NotImplementedException();

		public IEnumerable<LighthouseServiceProxy<T>> FindServices<T>() 
			where T : class, ILighthouseService
		{
			throw new NotImplementedException();
		}

		public bool TryConnect()
		{
			IsConnected = false;
			// TODO: yeahhhh nope
			var webclient = LighthouseContainer.GetNetworkProviders().First().GetWebClient();
			
			

			return true;
		}
	}
}
