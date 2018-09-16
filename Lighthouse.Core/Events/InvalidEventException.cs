using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Lighthouse.Core.Events
{
	[Serializable]
	internal class InvalidEventException : Exception
	{
		private Type foundType;
		private Type[] expected;

		InvalidEventException()
		{
		}

		InvalidEventException(string message) : base(message)
		{
		}

		public InvalidEventException(Type foundType, Type[] expected)
			: base($"Invalid event type {foundType} was provided. Expected: {string.Join(',', expected.Select(t => t.Name))}")
		{
			this.foundType = foundType;
			this.expected = expected;
		}

		protected InvalidEventException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}