using Lighthouse.Core.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.Hosting
{
	public static class NetworkProviderExtensions
	{
		public async static Task<TResponse> MakeRequest<TRequest, TResponse>(
			this INetworkProvider provider,
			Uri uri,
			TRequest request)
			where TRequest : class, new()
		{
			var requestWrapper = new LighthouseServerRequest<TRequest>(request);

			// serialize the request object 
			var serializedRequestObject = requestWrapper.SerializeForManagementInterface();

			return await provider.MakeRequest<TResponse>(uri, serializedRequestObject, true);
		}
	}
}
