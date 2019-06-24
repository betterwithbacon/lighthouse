using System.Collections.Generic;
using System.Collections.Immutable;

namespace Lighthouse.Core.Hosting
{
	public class LighthouseServerResponse<T>
	{
		public LighthouseServerStatus ServerStatus { get; private set; }

		public ImmutableDictionary<string,string> Attributes { get; private set; }

		public T Payload { get; private set; }

		public LighthouseServerResponse(LighthouseServerStatus serverStatus, T payload, IEnumerable<KeyValuePair<string, string>> attributes = null)
		{
			ServerStatus = serverStatus;
			Attributes = attributes?.ToImmutableDictionary();
			Payload = payload;
		}
	}

	public class LighthouseServerRequest<T>
		where T : class, new()
	{
		public T Request { get; set; }

		public LighthouseServerRequest(T request)
		{
			Request = request ?? new T();
		}
	}

    //public class ListServicesRequest
    //{
    //    public ServiceDescriptor ServiceDescriptorToFind { get; set; }
    //}

    //public enum LighthouseServicesResponseStatus
    //{
    //	Succeeded,
    //	Failed
    //}
}
