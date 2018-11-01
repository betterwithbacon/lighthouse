using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.IO
{
	public class InternetNetworkProvider : INetworkProvider
	{
		public ILighthouseServiceContainer LighthouseContainer { get; }

		public IList<NetworkScope> SupportedScopes { get; } = new List<NetworkScope> { NetworkScope.Internet, NetworkScope.Local };

		public IList<NetworkProtocol> SupportedProtocols { get; } = new List<NetworkProtocol> { NetworkProtocol.HTTP }; // for now only HTTP

		public InternetNetworkProvider(ILighthouseServiceContainer lighthouseContainer)
		{
			LighthouseContainer = lighthouseContainer;
		}

		public Task<string> GetStringAsync(Uri uri)
		{
			var client = new HttpClient();
			return client.GetStringAsync(uri);
		}

		public Task<byte[]> GetByteArrayAsync(Uri uri)
		{
			var client = new HttpClient();
			return client.GetByteArrayAsync(uri);
		}

		public async Task<T> GetObjectAsync<T>(Uri uri, bool throwErrors = false)
		{
			var rawText = await GetStringAsync(uri);
			T result = default;
			try
			{
				result = JsonConvert.DeserializeObject<T>(rawText);
				return result;
			}
			// TODO: make this less generic Exception, just catch ones that try to deserialze
			catch(Exception)
			{
				if (throwErrors)
					throw;	
			}

			return result;
		}
	}
}
