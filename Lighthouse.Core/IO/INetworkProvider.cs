using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Lighthouse.Core.IO
{
    public interface INetworkProvider
    {
		WebClient GetWebClient();
    }
}
