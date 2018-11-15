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

		IList<NetworkProtocol> SupportedProtocols { get; }

		Task<string> GetStringAsync(Uri uri);

		Task<byte[]> GetByteArrayAsync(Uri uri);

		Task<T> GetObjectAsync<T>(Uri uri, bool throwErrors = false);

		Task<T> MakeRequest<T>(Uri uri, string content, bool throwErrors = false);
	}
}
