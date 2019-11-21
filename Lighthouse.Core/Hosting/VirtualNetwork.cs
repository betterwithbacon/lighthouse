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

        public Task<T> GetObjectAsync<T>(Uri uri, bool throwErrors = false)
        {
            if (Containers.TryGetValue(uri, out var container))
            {
                return container.HandleRequest<WebRequestWrapper, T>(new WebRequestWrapper(uri));
            }
            
            // TODO: returning a default value might be bit misleading, but exceptions seems maybe too much. think this through?
            return default;
        }
    }

    public class WebRequestWrapper
    {
        public Uri Uri { get; }
        public string Payload { get; }
        public HttpMethod Method { get; }

        public WebRequestWrapper(Uri uri, HttpMethod method = null, string payload = null)
        {
            this.Uri = uri;
            this.Method = method;
            this.Payload = payload;
        }

    }
}
