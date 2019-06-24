using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

		public async Task<IEnumerable<LighthouseServiceProxy<T>>> FindServices<T>() 
			where T : class, ILighthouseService
		{
			var networkProvider = GetNetworkProvider();

			UriBuilder uriBuilder = new UriBuilder("http", RemoteServerAddress.ToString(), RemoteServerPort)
			{
				Path = LighthouseContainerCommunicationUtil.Endpoints.SERVICES
			};

			//// create a message to send to the remote server
			//var findServiceResponse = await networkProvider.MakeRequest<ListServicesRequest, LighthouseServerResponse<List<LighthouseServiceRemotingWrapper>>>(
			//		uriBuilder.Uri,
			//		new ListServicesRequest
			//		{
			//			ServiceDescriptorToFind =
			//				new ServiceDescriptor {
			//					Name = typeof(T).Name,
			//					Type = typeof(T).AssemblyQualifiedName // TODO: I'm doing this to keep compatability, but even minor changes in versions would break this, that might be an unintended "feature, but we probably want to beef this up
			//				}
			//		}
			//	);

			//var proxies = new List<LighthouseServiceProxy<T>>();

   //         // TODO: fail silently here, I guess <-- #pureLaziness
   //         if (findServiceResponse == null)
   //         {
   //             return proxies;
   //         }

   //         // convert service descriptors into proxies
   //         foreach (var serviceDescriptor in findServiceResponse.Payload)
			//{
			//	// TODO: add service resolution, to this. 
			//	//Technically, I'm not sure how you could request a service without it also being local, so this is more of a sanity check I think.

			//	var serviceType = Type.GetType(serviceDescriptor.ServiceTypeName, true);
			//	var proxyType = Type.GetType($"Lighthouse.Core.Hosting.LighthouseServiceProxy`1[[{serviceDescriptor.ServiceTypeName}]], Lighthouse.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", true);
			//	//var make Activator.CreateInstance(proxyType);
			//	//Type[] typeArgs = { serviceType };
			//	//var make = Act //MakeGenericType(serviceType);

			//	// TODO: this is SUUUUPER edgy, lets clean this up, once we know it works
			//	proxies.Add(Activator.CreateInstance(proxyType, this) as LighthouseServiceProxy<T>);
			//}

			return null;
		}

		private INetworkProvider GetNetworkProvider()
		{
			var networkProvider = Container
					.GetNetworkProviders()
					.FirstOrDefault((np) =>
						// find a network provider that can see the internal network AND communicate over HTTP
						np.SupportedScopes.Contains(NetworkScope.Local) &&
						np.SupportedProtocols.Contains(NetworkProtocol.HTTP)
					);

			if (networkProvider == null)
				throw new ApplicationException("No local HTTP network providers found.");

			return networkProvider;
		}

		public async Task<bool> TryConnect()
		{
			IsConnected = false;

			try
			{
				var networkProvider = GetNetworkProvider();

				UriBuilder uriBuilder = new UriBuilder("http", RemoteServerAddress.ToString(), RemoteServerPort)
				{
					Path = LighthouseContainerCommunicationUtil.Endpoints.PING
				};

				var serverStatus = await networkProvider.GetObjectAsync<LighthouseServerStatus>(uriBuilder.Uri);

				if (serverStatus != null)
				{
					ConnectionHistory.Add(new LighthouseServiceContainerConnectionStatus(Container.GetNow(), true, null));

					// this client will nwo be viewed as "connected"
					// TODO: probably could introduce some sort of more advanced protocol to determine the nature of the remote container, and is this connect "persistent". but not needed for a while methinks
					IsConnected = true; 
				}
				else
					ConnectionHistory.Add(new LighthouseServiceContainerConnectionStatus(Container.GetNow(), false, new Exception($"Could not contact server {uriBuilder.Uri}"))); // if the message is anything other than "OK" to a ping, then post the response or else a hard excption will be thrown.
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

		public async Task<ManagementInterfaceResponse> SubmitManagementRequest(ServerManagementRequestType requestType, IDictionary<string,object> requestParameters)
		{
			// serialize  the request and forward it to the remote target
			var networkProvider = GetNetworkProvider();
			UriBuilder uriBuilder = new UriBuilder("http", RemoteServerAddress.ToString(), RemoteServerPort)
			{
				Path = LighthouseContainerCommunicationUtil.Endpoints.MANAGEMENT 
			};

			var managementRequestResponse = await networkProvider.MakeRequest<ServerManagementRequest, ManagementInterfaceResponse>(
				uriBuilder.Uri, 
				new ServerManagementRequest
				{
					RequestType = requestType,					
					RequestParameters = requestParameters
				}
			);

			return managementRequestResponse;
		}		
	}

	public class ServerManagementRequest
	{
		public static class RequestTypes
		{
			public static class Install
			{
				public static class Arguments
				{
					public static string ServiceName = "SERVICE_NAME";
				}
			}
		}

		public ServerManagementRequestType RequestType { get; internal set; }
		public IDictionary<string, object> RequestParameters { get; internal set; }

		public ServerManagementRequest()
		{
			RequestParameters = new Dictionary<string, object>();
		}
	}
}
