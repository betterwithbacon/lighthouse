using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Lighthouse.Core.IO;

namespace Lighthouse.Core.Hosting
{
    public class VirtualNetwork : INetworkProvider
    {
        public Dictionary<Uri, ILighthouseServiceContainer> Containers { get; } = new Dictionary<Uri, ILighthouseServiceContainer>();

        public IList<NetworkScope> SupportedScopes => new[] { NetworkScope.Local };

        public void Register(ILighthouseServiceContainer container, Uri uri)
        {
             if (Containers.ContainsKey(uri))
            {
                throw new InvalidOperationException("URI is alreay in use");
            }

            Containers.Add(uri, container);
        }

        public bool Deregister(Uri uri) => Containers.Remove(uri);

        public async Task<TResponse> GetObjectAsync<TRequest, TResponse>(Uri uri, TRequest requestObject, bool throwErrors = false)
            where TRequest : class
        {
            if (!Containers.TryGetValue(uri, out var container))
            {
                // TODO: returning a default value might be bit misleading, but exceptions seems maybe too much. think this through?
                // return await Task.FromResult<TResponse>(default);
                throw new Exception($"URI not found {uri}");
            }
            return await container.HandleRequest<TRequest, TResponse>(requestObject);
        }
    }
}
