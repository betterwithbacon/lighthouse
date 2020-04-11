using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.IO
{
	public interface INetworkProvider : IResourceProvider
    {
        Task<TResponse> GetObjectAsync<TRequest, TResponse>(Uri uri, TRequest requestObject, bool throwErrors = false)
            where TRequest : class;

        bool CanResolve(string address);

        ILighthouseClient GetClient(string address);
    }
}
