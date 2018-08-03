using System;
using System.Runtime.Serialization;

namespace Lighthouse.Core.UI
{
	[Serializable]
	public class InvalidCommandException : Exception
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

		public InvalidCommandException(string invalidCommand)
		{
			InvalidCommand = invalidCommand;
		}

		public string InvalidCommand { get; internal set; }
	}

	[Serializable]
	public class InvalidCommandArgumentException : Exception
	{
		InvalidCommandArgumentException()
		{
		}

		InvalidCommandArgumentException(string message, Exception innerException) : base(message, innerException)
		{
		}

		InvalidCommandArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public InvalidCommandArgumentException(string invalidArgument)
		{
			InvalidArgument = invalidArgument;
		}

		public string InvalidArgument { get; internal set; }
	}
}