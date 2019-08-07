using Lighthouse.Core.Configuration.Formats.Memory;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Lighthouse.Core.Hosting
{
	//public class LighthouseServerResponse<T>
	//{

	//	public ImmutableDictionary<string,string> Attributes { get; private set; }

	//	public T Payload { get; private set; }

	//	public LighthouseServerResponse(T payload, IEnumerable<KeyValuePair<string, string>> attributes = null)
	//	{
	//		ServerStatus = serverStatus;
	//		Attributes = attributes?.ToImmutableDictionary();
	//		Payload = payload;
	//	}
	//}

	//public class LighthouseServerRequest<T>
	//	where T : class, new()
	//{
	//	public T Request { get; set; }

	//	public LighthouseServerRequest(T request)
	//	{
	//		Request = request ?? new T();
	//	}
	//}
}
