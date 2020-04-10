using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Lighthouse.Core.IO;
using Lighthouse.Core.Utils;
using System.Linq;

namespace Lighthouse.Core.Hosting
{
    public class VirtualNetwork : INetworkProvider
    {
        public ResourceProviderType Type => ResourceProviderType.Network;
        public const string DesiredUriKey = "desiredUri";
        private int highestSubdomain = 1;

        public Dictionary<Uri, ILighthouseServiceContainer> Containers { get; } = new Dictionary<Uri, ILighthouseServiceContainer>();

        public IList<NetworkScope> SupportedScopes => new[] { NetworkScope.Local };

        public Uri ResolveUri(ILighthouseServiceContainer peer)
        {
            foreach(var uriAndPeer in Containers)
            {
                if (uriAndPeer.Value == peer)
                    return uriAndPeer.Key;
            }

            return null;
        }

        public async Task<TResponse> GetObjectAsync<TRequest, TResponse>(Uri uri, TRequest requestObject, bool throwErrors = false)
            where TRequest : class
        {
            if (!Containers.TryGetValue(uri, out var container))
            {
                throw new Exception($"URI not found {uri}");
            }
            return await container.HandleRequest<TRequest, TResponse>(requestObject);
        }

        public void Register(ILighthouseServiceContainer node, Dictionary<string,string> otherConfig = null)
        {
            if (!Containers.ContainsValue(node))
            {
                if (otherConfig?.ContainsKey(DesiredUriKey) ?? false)
                {
                    if(Containers.ContainsKey(DesiredUriKey.ToUri()))
                    {
                        throw new ApplicationException("Desired URI is already taken.");
                    }

                    Containers.Add($"http://127.0.0.{highestSubdomain++}".ToUri(), node);
                }
                else
                {
                    // incrememt the URI (these addresses are just automatically assigned, low-rent DHCP)
                    Containers.Add($"http://127.0.0.{highestSubdomain++}".ToUri(), node);
                }
            }
        }

        public override string ToString()
        {
            return $"Virtual Network ({Containers.Count} peers)";
        }

        public IEnumerable<ILighthouseServiceContainer> GetLighthousePeers()
        {
            return Containers.Values;
        }
    }
}
