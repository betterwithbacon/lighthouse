using Lighthouse.Core.Logging;
using Lighthouse.Core.Utils;
using System;

namespace Lighthouse.Core.Events
{
	public abstract class BaseEventProducer : IEventProducer
	{
		public string Identifier { get; private set; }

		public ILighthouseServiceContainer LighthouseContainer { get; protected set; }

		public void Init(ILighthouseServiceContainer container)
		{
			LighthouseContainer = container;
			Identifier = LighthouseComponentLifetime.GenerateSessionIdentifier(this);
			LighthouseContainer.Log(LogLevel.Debug, LogType.ProducerStartup,this);
			Start();
		}

		public abstract void Start();

		protected virtual void AssertIsReady()
		{
			if (LighthouseContainer == null)
				throw new InvalidOperationException("Context is not set.");
		}

		public override string ToString()
		{
			return Identifier;
		}
	}
}
