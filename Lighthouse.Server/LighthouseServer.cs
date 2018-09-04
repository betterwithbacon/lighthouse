using BusDriver.Core.Events;
using BusDriver.Core.Logging;
using Lighthouse.Core;
using Lighthouse.Core.Deployment;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lighthouse.Server
{
    public class LighthouseServer : ILighthouseServiceContainer, ILogSource
	{
		public class LighthouseServiceRun
		{
			public readonly ILighthouseService Service;
			public readonly IProducerConsumerCollection<Exception> Exceptions = new ConcurrentBag<Exception>();
			internal readonly Task Task;

			public LighthouseServiceRun(ILighthouseService service, Task task)
			{
				Service = service;
				Task = task;
			}
		}

		private readonly ConcurrentBag<LighthouseServiceRun> ServiceThreads = new ConcurrentBag<LighthouseServiceRun>();		
		private readonly Action<string> LogLocally;		
		private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
		public event StatusUpdatedEventHandler StatusUpdated;
		public bool IsRunning { get; private set; }
		public IEventContext EventContext { get; private set; }
		ILighthouseServiceContainer ILighthouseComponent.LighthouseContainer => this;
		public string Identifier => throw new NotImplementedException();
		
		public LighthouseServer(Action<string> localLogger, IEventContext eventContext = null)
		{
			LogLocally = localLogger;
			EventContext = eventContext ?? new EventContext();
		}

		public void Start()
		{
			Log(LogLevel.Debug,this, "Lighthouse server starting");

			StartMonitor();
			
			IsRunning = true;
		}

		public void Stop()
		{
			// call stop on all of the services
			ServiceThreads.ToList().ForEach(serviceRun => { serviceRun.Service.Stop(); });

			int iter = 1;
			
			// just do some kindness before killing the thread. Most thread terminations should be instantaneous.
			while(GetRunningServices().Any() && iter < 3)
			{
				Log(LogLevel.Debug, this, $"[Stopping] Waiting for services to finish. attempt {iter}");
				Thread.Sleep(500);
				iter++;
			}

			// wait some period of time, for the services to stop by themselves, then just kill the threads out right.
			CancellationTokenSource.Cancel();

			Log(LogLevel.Debug,this, "[Lighthouse Server Stopped]");
		}

		void AssertIsRunning()
		{
			if (!IsRunning)
				throw new InvalidOperationException("Lighthouse server is not running.");	
		}

		private void StartMonitor()
		{
			// the unhandled exceptions from tasks, will be handled by the lighthouse runtime here
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

			Log(LogLevel.Debug, this, "Lighthouse Monitor Started");
		}

		private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			// log the failure (where's the taskId?)
			HandleTaskError(e.Exception);

			// mark this as having "been seen"
			e.SetObserved();
		}

		private void HandleTaskError(Exception e, string serviceId = null)
		{
			LighthouseServiceRun owner = null; 
			if (serviceId != null)
			{
				owner = ServiceThreads.FirstOrDefault(ls => ls.Service.Id == serviceId);
				owner?.Exceptions.TryAdd(e);
			}

			Log(LogLevel.Error, owner?.Service, $"Error occurred running task: {e.Message}");
		}

		public void Launch(IEnumerable<LighthouseAppLaunchConfig> launchConfigs)
		{
			foreach(var launchConfig in launchConfigs)
			{
				Launch(launchConfig);
			}
		}

		public void Launch(LighthouseAppLaunchConfig launchConfig)
		{
			AssertIsRunning();

			Log(LogLevel.Debug, this, $"Attempting to start app: {launchConfig.Name} (ID: {launchConfig.Id}).");

			if (!(Activator.CreateInstance(launchConfig.Type) is ILighthouseService service))
				throw new ApplicationException($"App launch config doesn't represent Lighthouse app. {launchConfig.Type.AssemblyQualifiedName}");

			Launch(service);
		}

		public void Launch(ILighthouseService service)
		{
			AssertIsRunning();

			// put the service in a runnable state			
			service.StatusUpdated += Service_StatusUpdated;
			service.Initialize(this);

			// start it, in a separate thread, that will run the business logic for this
			var startedTask = Task.Run(() => service.Start(), CancellationTokenSource.Token).ContinueWith(
				(task) =>
				{
					// handle errors
					if (task.IsFaulted)
					{ 						
						HandleTaskError(task.Exception.InnerException);
					}
					else
					{
						HandleTaskCompletion(task);
					}
				}, CancellationTokenSource.Token); // fire and forget

			ServiceThreads.Add(new LighthouseServiceRun(service, startedTask));
		}

		private void HandleTaskCompletion(Task task)
		{
			Log(LogLevel.Debug, this, $"App completed successfully. {ServiceThreads.FirstOrDefault(lsr => lsr.Task == task)?.Service}");
		}

		private void Service_StatusUpdated(ILighthouseComponent owner, string status)
		{
			Log(LogLevel.Debug, owner, status);
		}

		void RaiseStatusUpdated(string statusChangeMessage)
		{
			StatusUpdated?.Invoke(this, statusChangeMessage);
		}

		public void Log(LogLevel level, ILighthouseComponent sender, string message)
		{
			string log = $"[{DateTime.Now.ToString("HH:mm:fff")}] [{sender}]: {message}";

			// ALL messages are logged locally for now
			LogLocally(log);

			// emit the messages in the event context as well, so it can be reacted to there as well
			EventContext?.Log(BusDriver.Core.Logging.LogType.Info, message, sender is ILogSource ls ? ls : this);
		}

		public IEnumerable<LighthouseServiceRun> GetRunningServices()
			=> ServiceThreads.Where(s => s.Service.RunState > LighthouseServiceRunState.PendingStart && s.Service.RunState < LighthouseServiceRunState.PendingStop);

		public override string ToString()
		{
			return "Lighthouse Server";
		}

		public IEnumerable<T> FindServices<T>() where T : ILighthouseService
		{
			return ServiceThreads.Select(st => st.Service).OfType<T>();
		}

		public IEnumerable<T> FindRemoteServices<T>() where T : ILighthouseService
		{
			return Enumerable.Empty<T>();
		}

		public DateTime GetTime()
		{
			// for now, just use local time, but this should eventually use UTC
			return DateTime.Now;
		}

		public IEnumerable<T> GetResourceProviders<T>() where T : IResourceProvider
		{
			
		}

		public IEnumerable<IFileSystemProvider> GetFileSystemProviders()
		{
			return GetResourceProviders<IFileSystemProvider>();
		}
	}
}