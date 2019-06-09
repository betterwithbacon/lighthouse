using Lighthouse.Core.Logging;
using Lighthouse.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Lighthouse.Core.Storage;

namespace Lighthouse.Core.Events
{
	public abstract class BaseEventConsumer : IEventConsumer, IStorageScope, ILighthouseComponent
	{		
		public abstract IList<Type> Consumes { get; }

		public ILighthouseServiceContainer Container { get; protected set; }

		public string ScopeName => Identifier;

		public string Identifier { get; protected set; }

		private readonly IReadOnlyDictionary<Type, MethodInfo> MethodCache;

		public BaseEventConsumer()
		{
			MethodCache = new ReadOnlyDictionary<Type, MethodInfo>(
				ReflectionUtil.GetMethodsBySingleParameterType(this.GetType(), "HandleEvent")
			);

			// Get all the methods named HandleEvent, then fijnd the ones with just ONE argument, and then separate them by type
			// TODO: this doesn't handle 
		}

		public bool Equals(IStorageScope x, IStorageScope y)
		{
			return x.ScopeName == y.ScopeName && x.Identifier == y.Identifier;
		}

		public int GetHashCode(IStorageScope obj)
		{
			return HashCode.Combine(obj.Identifier, obj.ScopeName);
		}

		public virtual void HandleEvent(IEvent ev)
		{
			Container.Log(LogLevel.Debug, LogType.EventReceived, this, message: ev.ToString());

			// Find a type specific implementation on the inheriter.
			// if not, then this method should be overwritten
			if (MethodCache.TryGetValue(ev?.GetType(), out var method))
				method.Invoke(this, new[] { ev });
		}

		public void Init(ILighthouseServiceContainer container)
		{
			Container = container;
			Identifier = Utils.LighthouseComponentLifetime.GenerateSessionIdentifier(this);
			Container.Log(LogLevel.Debug, LogType.ConsumerStartup, this);
			OnInit();
		}

		/// <summary>
		/// Operations that should be performed after the initial state fo the consumer is initialized.
		/// The container and other base propertiues should be defined at this point.
		/// </summary>
		protected virtual void OnInit() { }

		public override string ToString()
		{
			return Identifier;
		}
	}
}
