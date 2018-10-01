using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Lighthouse.Core.IO
{
	public class InternetNetworkProvider : INetworkProvider
	{	
		public ILighthouseServiceContainer LighthouseContainer { get; }

		public InternetNetworkProvider(ILighthouseServiceContainer lighthouseContainer)
		{
			LighthouseContainer = lighthouseContainer;
		}

		public WebClient GetWebClient()
		{
			return new WebClient();
		}
	}
}
