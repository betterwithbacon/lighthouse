﻿using Lighthouse.Core.Scheduling;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lighthouse.Core
{
    public abstract class LighthouseServiceBase : ILighthouseService
    {	
		public string Id { get; private set; }
		public LighthouseServiceRunState RunState { get; protected set; }
		public ILighthouseServiceContainer LighthouseContainer { get; private set; }

		public event StatusUpdatedEventHandler StatusUpdated;		
		private readonly List<Action<ILighthouseServiceContainer>> StartupActions = new List<Action<ILighthouseServiceContainer>>();
		
		protected void AddStartupTask(Action<ILighthouseServiceContainer> task)
		{
			StartupActions.Add(task);
		}

		protected void AddScheduledTask(Schedule schedule, Action<DateTime> taskToPerform)
		{
			LighthouseContainer.AddScheduledAction(schedule, taskToPerform);
		}

		public virtual void Start()
        {
			if (LighthouseContainer == null)
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
			StartupActions.ForEach(LighthouseContainer.Do);			
		}

		protected virtual void OnStart()
		{
		}
		
		protected virtual void OnAfterStart()
		{
		}

		public virtual void Stop()
		{
			RaiseStatusUpdated(LighthouseServiceRunState.PendingStop);
			StatusUpdated?.Invoke(this, $"Service shutting down.");
			OnStop();
			RaiseStatusUpdated(LighthouseServiceRunState.Stopped);
		}

		protected virtual void OnStop()
		{
		}

		protected void RaiseStatusUpdated(LighthouseServiceRunState newState)
		{
			StatusUpdated?.Invoke(this, $"Status changing from {RunState} to {newState}");
			RunState = newState;
		}

		protected void RaiseStatusUpdated(string message)
		{
			StatusUpdated?.Invoke(this, message);			
		}

		public void Initialize(ILighthouseServiceContainer context)
		{
			LighthouseContainer = context;
			//Id = EventContext.GenerateSessionIdentifier(this);
			RaiseStatusUpdated(LighthouseServiceRunState.PendingStart);
		}

		public override string ToString()
		{
			return $"[{GetType().Name}|{Id}]";
		}
	}
}