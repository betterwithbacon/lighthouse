using System;

namespace Lighthouse.Core.Events
{
	public abstract class BaseEventProducer : IEventProducer
	{
		public string Identifier { get; private set; }

		public string LogDescriptor => Identifier;

		protected IEventContext Context { get; private set; }

		public void Init(IEventContext context)
		{
			Context = context;
			Identifier = EventContext.GenerateSessionIdentifier(this);
			//Context.Log(LogType.ProducerStartup, source: this);

			Start();
		}

		public abstract void Start();

		protected virtual void AssertIsReady()
		{
			if (Context == null)
				throw new InvalidOperationException("Context is not set.");
		}

		public override string ToString()
		{
			return Identifier;
		}

		public string GetContextId()
		{
			return Context.Id;
		}
	}
}
