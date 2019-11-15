using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lighthouse.Core.IO;

namespace Lighthouse.Core.Hosting
{
    public class VirtualNetwork : INetworkProvider
    {
        public Dictionary<Uri, ILighthouseServiceContainer> Containers { get; } = new Dictionary<Uri, ILighthouseServiceContainer>();

        public IList<NetworkScope> SupportedScopes => new[] { NetworkScope.Local };

        public IList<NetworkProtocol> SupportedProtocols => new[] { NetworkProtocol.TCP };

        public Task<byte[]> GetByteArrayAsync(Uri uri)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetObjectAsync<T>(Uri uri, bool throwErrors = false)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetStringAsync(Uri uri)
        {
            throw new NotImplementedException();
        }

        public Task<T> MakeRequest<T>(Uri uri, string content, bool throwErrors = false)
        {
            throw new NotImplementedException();
        }
    }
}
