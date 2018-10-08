using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Hosting
{
	public sealed class LighthouseContainerCommunicationUtil
	{
		public const int DEFAULT_SERVER_PORT = 54545;
		public static class Paths
		{
			public const string PING = @"\PING";
		}		

		public static class Messages
		{
			public const string OK = @"OK";
		}
	}
}
