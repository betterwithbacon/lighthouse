using Lighthouse.Core;
using System;
using System.Threading.Tasks;

namespace Lighthouse.Client
{
    public class LighthouseClient : ILighthousePeer
    {
        public Task<TResponse> MakeRequest<TRequest, TResponse>(TRequest request) where TRequest : class
        {
            return null;
        }
    }
}
