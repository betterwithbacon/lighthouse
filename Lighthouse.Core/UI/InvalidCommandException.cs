using System;
using System.Runtime.Serialization;

namespace Lighthouse.Core.UI
{
	[Serializable]
	internal class InvalidCommandException : Exception
	{
		InvalidCommandException()
		{
		}

		InvalidCommandException(string message, Exception innerException) : base(message, innerException)
		{
		}

		InvalidCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public InvalidCommandException(string invalidArgument)
		{
			InvalidArgument = invalidArgument;
		}

		public string InvalidArgument { get; internal set; }
	}
}