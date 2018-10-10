using Lighthouse.Core.Management;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Hosting
{
	public sealed class LighthouseContainerCommunicationUtil
	{
		public const int DEFAULT_SERVER_PORT = 54545;

		public static class Endpoints
		{
			public const string PING = @"PING";
			public const string SERVICES = @"SERVICES";
		}

		public static class Messages
		{
			public const string OK = @"OK";
		}
	}

	public static class LighthouseContainerCommunicationUtilExtensions
	{
		public static string SerializeForManagementInterface(this object lighthouseServerStatus)
		{
			return JsonConvert.SerializeObject(lighthouseServerStatus);
		}

		public static T DeserializeForManagementInterface<T>(this string serializedText)
		{
			return JsonConvert.DeserializeObject<T>(serializedText);
		}

		public static ManagementInterfaceResponse ToMIResponse(this string serializedMI)
		{
			return new ManagementInterfaceResponse(true, serializedMI);
		}
	}
}
