﻿using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.Hosting
{
	public class NetworkLighthouseServiceContainerConnection : ILighthouseServiceContainerConnection
	{	
		public bool IsConnected { get; private set; } = false;

		public ILighthouseServiceContainer Container { get; }

		public ILighthouseServiceContainer RemoteContainer
			=> throw new Exception("Can't retrieve direct container on a unidirectional connection. Use FindServices to retrieve components from the remote container.");
		
		public IPAddress RemoteServerAddress { get;}

		public IList<LighthouseServiceContainerConnectionStatus> ConnectionHistory { get; } =
			new List<LighthouseServiceContainerConnectionStatus>();

		public bool IsBidirectional => false; // for now, this will NOT provide bidirectional communication, because all sorts of networking implications will be in place

		public int RemoteServerPort { get; private set; }

		public NetworkLighthouseServiceContainerConnection(ILighthouseServiceContainer localContainer, IPAddress ipAddress, int port = LighthouseContainerCommunicationUtil.DEFAULT_SERVER_PORT)
		{
			Container = localContainer;
			RemoteServerAddress = ipAddress;			
			RemoteServerPort = port;
		}

		public IEnumerable<LighthouseServiceProxy<T>> FindServices<T>() 
			where T : class, ILighthouseService
		{
			throw new NotImplementedException();
		}

		public async Task<bool> TryConnect()
		{
			IsConnected = false;

			try
			{	
				var networkProvider = Container
					.GetNetworkProviders()
					.FirstOrDefault((np) => 
						// find a network provider that can see the internal network AND communicate over HTTP
						np.SupportedScopes.Contains(IO.NetworkScope.Local) && 
						np.SupportedProtocols.Contains(IO.NetworkProtocol.HTTP)
					);

				if (networkProvider == null)
					throw new ApplicationException("No local HTTP network providers found.");

				UriBuilder uriBuilder = new UriBuilder("http", RemoteServerAddress.ToString(), RemoteServerPort);
				uriBuilder.Path = LighthouseContainerCommunicationUtil.Endpoints.PING;

				var response = await networkProvider.GetStringAsync(uriBuilder.Uri);

				if (response == LighthouseContainerCommunicationUtil.Messages.OK)
				{
					ConnectionHistory.Add(new LighthouseServiceContainerConnectionStatus(Container.GetNow(), true, null));

					// this client will nwo be viewed as "connected"
					// TODO: probably could introduce some sort of more advanced protocol to determine the nature of the remote container, and is this connect "persistent". but not needed for a while methinks
					IsConnected = true; 
				}
				else
					ConnectionHistory.Add(new LighthouseServiceContainerConnectionStatus(Container.GetNow(), false, new Exception(response))); // if the message is anything other than "OK" to a ping, then post the response or else a hard excption will be thrown.
			}
			catch(Exception e)
			{
				IsConnected = false;
				ConnectionHistory.Add(new LighthouseServiceContainerConnectionStatus(Container.GetNow(), false, e));
				Container.Log(LogLevel.Debug, LogType.Error, this, $"Connection to {RemoteServerAddress?.ToString() ?? "<no server>" } could not be made.", exception: e, emitEvent: false);
				throw;
			}

			return IsConnected;
		}
	}
}
