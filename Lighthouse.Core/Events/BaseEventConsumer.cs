using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseCore;

namespace Lighthouse.Core.Events
{
	public abstract class BaseEventConsumer : IEventConsumer, IStorageScope, ILighthouseComponent
	{		
		public abstract IList<Type> Consumes { get; }

		public ILighthouseServiceContainer LighthouseContainer { get; protected set; }

		public string ScopeName => Identifier;

		public string Identifier { get; private set; }

		public event StatusUpdatedEventHandler StatusUpdated;

		public bool Equals(IStorageScope x, IStorageScope y)
		{
			return x.ScopeName == y.ScopeName && x.Identifier == y.Identifier;
		}

		public int GetHashCode(IStorageScope obj)
		{
			return HashCode.Combine(obj.Identifier, obj.ScopeName);
		}

		public void HandleEvent(IEvent ev)
		{
			throw new NotImplementedException();
		}

		public void Init(ILighthouseServiceContainer container)
		{
			LighthouseContainer = container;
			Identifier = Utils.LighthouseComponentLifetime.GenerateSessionIdentifier(this);
			LighthouseContainer.Log(LogLevel.Debug, LogType.ConsumerStartup, this);
			OnInit();
		}

		/// <summary>
		/// Operations that should be performed after the initial state fo the consumer is initialized.
		/// The container and other base propertiues should be defined at this point.
		/// </summary>
		protected abstract void OnInit();		
	}
}
