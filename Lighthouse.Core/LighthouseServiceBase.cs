﻿using Lighthouse.Core.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lighthouse.Core
{
    public abstract class LighthouseServiceBase : ILighthouseService
	{
		public string Id { get; private set; }
		public ILighthouseServiceContainer Container { get; private set; }		
		private readonly List<Action<ILighthouseServiceContainer>> StartupActions = new List<Action<ILighthouseServiceContainer>>();
		protected virtual bool IsInitialized { get; }
		public virtual string Identifier => GetServiceIdentifier();

		protected void AddStartupTask(Action<ILighthouseServiceContainer> task)
		{
			StartupActions.Add(task);
		}

		public async Task Start()
		{
			if (Container == null)
				throw new InvalidOperationException("Service not initialized. (No container set)");

			await OnStart();			
			await OnAfterStart();
			await PerformStartupTasks();
            await Task.CompletedTask;
		}

        public async Task Stop()
        {
            await OnStop();
        }

        private Task PerformStartupTasks()
		{
			// the context, will do the work for us
			// this is useful, because if some of the things you want to do will be emitting Events, then they'll be picked up
			StartupActions.ForEach((a) => Container.Do(a, "Perform startup task."));
            return Task.CompletedTask;
        }

		#region Service Lifecycle Events
		protected virtual Task OnStart()
		{
            return Task.CompletedTask;
		}

		protected virtual Task OnAfterStart()
		{
            return Task.CompletedTask;
        }

		protected virtual Task OnStop()
		{
            return Task.CompletedTask;
        }
		#endregion

		public void Initialize(ILighthouseServiceContainer container, object context = null)
		{
			if (IsInitialized)
				return;

            // if this has a service moniker us that, otherwise just for got type
            Id = this.ExternalServiceName() ?? GetType().Name;

			Container = container;

			OnInit(context);			
		}

		protected virtual void OnInit(object context = null)
		{

		}

		public override string ToString()
		{
			return $"[{GetType().Name}|{Id}]";
		}


		protected virtual string GetServiceIdentifier()
		{
			return GetType().Name;
		}
    }
}