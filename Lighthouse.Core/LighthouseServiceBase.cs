using Lighthouse.Core.Scheduling;
using Lighthouse.Core.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lighthouse.Core
{
	public abstract class LighthouseServiceBase : ILighthouseService, IStorageScope
	{
		public string Id { get; private set; }
		public LighthouseServiceRunState RunState { get; protected set; }
		public ILighthouseServiceContainer Container { get; private set; }		
		private readonly List<Action<ILighthouseServiceContainer>> StartupActions = new List<Action<ILighthouseServiceContainer>>();
		protected virtual bool IsInitialized { get; }
		public string ScopeName => Identifier;
		public string Identifier => GetServiceIdentifier();

		protected void AddStartupTask(Action<ILighthouseServiceContainer> task)
		{
			StartupActions.Add(task);
		}

		public virtual void Start()
		{
			if (Container == null)
				throw new InvalidOperationException("Service not initialized. (No container set)");

			OnStart();
			RaiseStatusUpdated(LighthouseServiceRunState.Running);
			OnAfterStart();
			PerformStartupTasks();
		}

		private void PerformStartupTasks()
		{
			// the context, will do the work for us
			// this is useful, because if some of the things you want to do will be emitting Events, then they'll be picked up
			StartupActions.ForEach((a) => Container.Do(a, "Perform startup task."));
		}

		#region Service Lifecycle Events
		protected virtual void OnStart()
		{
		}

		protected virtual void OnAfterStart()
		{
		}

		public virtual void Stop()
		{
			RaiseStatusUpdated(LighthouseServiceRunState.PendingStop);			
			OnStop();
			RaiseStatusUpdated(LighthouseServiceRunState.Stopped);
		}

		protected virtual void OnStop()
		{
		}
		#endregion


		protected void RaiseStatusUpdated(LighthouseServiceRunState newState)
		{	
			RunState = newState;
		}

		public void Initialize(ILighthouseServiceContainer context)
		{
			if (IsInitialized)
				return;

			Container = context;
			OnInit();
			RaiseStatusUpdated(LighthouseServiceRunState.PendingStart);
		}

		protected virtual void OnInit()
		{

		}

		public override string ToString()
		{
			return $"[{GetType().Name}|{Id}]";
		}

		public bool Equals(IStorageScope x, IStorageScope y)
		{
			return x.Identifier == y.Identifier;
		}

		public int GetHashCode(IStorageScope obj)
		{
			return obj.Identifier.GetHashCode();
		}

		protected virtual string GetServiceIdentifier()
		{
			return GetType().Name;
		}
	}
}