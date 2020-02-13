using Newtonsoft.Json;

namespace Lighthouse.Core.Hosting
{
	public sealed class LighthouseContainerCommunicationUtil
	{
		public const int DEFAULT_SERVER_PORT = 54546;
		public const string LOCAL_SERVER_ADDRESS = "127.0.0.1";
		
		public static class Endpoints
		{
			public const string PING = @"PING";
			public const string SERVICES = @"SERVICES";
			public const string MANAGEMENT = @"MANAGEMENT";
		}

		public static class Messages
		{
			public const string OK = @"OK";
			public const string ERROR = @"ERROR";
			public const string UNSUPPORTED = @"UNSUPPORTED";			
		}
	}

	
}
