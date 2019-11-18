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

        public Task<T> GetObjectAsync<T>(Uri uri, bool throwErrors = false)
        {
            throw new NotImplementedException();
        }

        public Task<T> MakeRequest<T>(Uri uri, string content, bool throwErrors = false)
        {
            throw new NotImplementedException();
        }
    }
}
