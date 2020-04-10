using Lighthouse.Core.Utils;
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
		public IList<NetworkScope> SupportedScopes { get; } = new List<NetworkScope> { NetworkScope.Internet, NetworkScope.Local };
        public IList<NetworkProtocol> SupportedProtocols { get; } = new List<NetworkProtocol> { NetworkProtocol.HTTP }; // for now only HTTP

        public ResourceProviderType Type => ResourceProviderType.Network;

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

		//public async Task<T> GetObjectAsync<T>(Uri uri, bool throwErrors = false)
		//{
		//	var rawText = await GetStringAsync(uri);
		//	return rawText.ConvertJsonToTarget<T>(throwErrors);
		//}

        public async Task<TResponse> GetObjectAsync<TRequest, TResponse>(Uri uri, TRequest requestObject, bool throwErrors = false) where TRequest : class
        {
            using (var client = new HttpClient())
            {
                var responseContent = (await client.SendAsync(
                    new HttpRequestMessage(HttpMethod.Post, uri)
                    {
                        Content = new StringContent(requestObject.ConvertToJson())
                    })).Content;
                return (await responseContent.ReadAsStringAsync()).ConvertJsonToTarget<TResponse>(throwErrors);
            }
        }

        public void Register(ILighthouseServiceContainer peer, Dictionary<string, string> otherConfig = null)
        {
            // no op, it doesn't care here either
        }

        public IEnumerable<ILighthouseServiceContainer> GetLighthousePeers()
        {
            // not sure how this will scan the network looking for peers
            throw new NotImplementedException();
        }
    }
}
