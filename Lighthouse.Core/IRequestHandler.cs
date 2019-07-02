using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core
{
    public interface IRequestHandler<TRequest, TResponse> : IRequestHandler
    {
        TResponse Handle(TRequest request);
    }

    public interface IRequestHandler
    {
    }
}
