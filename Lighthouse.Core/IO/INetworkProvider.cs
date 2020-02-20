using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.IO
{
	public interface INetworkProvider : IResourceProvider
    {
		IList<NetworkScope> SupportedScopes { get; }

        Task<TResponse> GetObjectAsync<TRequest, TResponse>(Uri uri, TRequest requestObject, bool throwErrors = false)
            where TRequest : class;

        IEnumerable<ILighthousePeer> GetLighthousePeers();
    }
}
