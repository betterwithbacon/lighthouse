using BusDriver.Core.Events;
using BusDriver.Core.Logging;
using Lighthouse.Core;
using Lighthouse.Core.Configuration;
using Lighthouse.Core.Configuration.Providers;
using Lighthouse.Core.Configuration.Providers.Local;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Deployment;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using Lighthouse.Monitor;
using Lighthouse.Server.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Lighthouse.Server
{
    public class LighthouseServer : ILighthouseServiceContainer, ILighthouseLogSource, ILogSource
	{
		#region LighthouseServiceRun
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
		#endregion

		#region Fields
		public const string DEFAULT_APP_NAME = "Lighthouse Server";
		private readonly ConcurrentBag<LighthouseServiceRun> ServiceThreads = new ConcurrentBag<LighthouseServiceRun>();		
		private readonly Action<string> LogLocally;		
		private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
		public event StatusUpdatedEventHandler StatusUpdated;

		public bool IsRunning { get; private set; }
		public IEventContext EventContext { get; private set; }		
		public string Identifier => throw new NotImplementedException();
		public string WorkingDirectory { get; private set; } = @"C:\";

		public IAppConfigurationProvider LaunchConfiguration { get; private set; }

		// Local cache of ALL repositories. this will likely include more than the initial config
		private IList<IServiceRepository> ServiceRepositories { get; set; }

		public LighthouseMonitor LighthouseMonitor { get; private set; }
		public readonly OSPlatform OS;
		#endregion

		#region Constructors
		public LighthouseServer(Action<string> localLogger, IAppConfigurationProvider launchConfiguration = null, IEventContext eventContext = null, string workingDirectory = null)
		{
			LogLocally = localLogger;
			EventContext = eventContext ?? new EventContext();
			ServiceRepositories = new List<IServiceRepository>();
			LaunchConfiguration = launchConfiguration ?? new MemoryAppConfigurationProvider("lighthouse ", this); // if no config is passed in, start with a blank one

			//TODO: this seems a little hacky, but I DO want to eventually enforce graph participation by components
			// e.g.: if a component tries to log, it needs to be registered with this container
			if (LaunchConfiguration.LighthouseContainer != this)
				RegisterComponent(LaunchConfiguration);

			// set the local environment state
			OS = RuntimeServices.GetOS();
			WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory;

			Log(LogLevel.Debug, this, "Lighthouse server initializing...");

			// load up the providers
			LoadProviders();
		}
		#endregion

		#region Server Lifecycle
		public void Start()
		{
			Log(LogLevel.Debug,this, "Lighthouse server starting");

			// start the Mointor service. This will make sure everything is working correctly.
			StartMonitor();

			// load up the local resources						
			// look for the local service config
			LoadConfiguration();			
			LaunchConfiguredServices();

			// the service is now runnable
			IsRunning = true;
		}

		private void LaunchConfiguredServices()
		{
			// TODO: right now, it's one, but it COULD be more, what's that like?!
			
			// reigster all of the service requests, from the config providers.
			foreach (var request in LaunchConfiguration.GetServiceLaunchRequests())
			{
				Log(LogLevel.Debug, this, $"Registering service request: {request}");
				LighthouseMonitor.RegisterServiceRequest(request);

				// launch the service
				Launch(request);
			}
		}

		public void LoadConfiguration()
		{
			// TODO: there should probably be only one local appconfig resource
			LaunchConfiguration = GetResourceProviders<IAppConfigurationProvider>().FirstOrDefault();
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

			LighthouseMonitor = new LighthouseMonitor();
		}
		#endregion

		#region Error Handling
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
		#endregion

		#region Service Launching
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

		public void Launch(ServiceLaunchRequest launchRequest)
		{
			AssertIsRunning();

			Log(LogLevel.Debug, this, $"Attempting to start service: {launchRequest.Name}.");

			if (!(Activator.CreateInstance(launchRequest.ServiceType) is ILighthouseService service))
				throw new ApplicationException($"App launch config doesn't represent Lighthouse app. {launchRequest.ServiceType.AssemblyQualifiedName}");

			Launch(service);
		}

		public void Launch(ILighthouseService service)
		{
			AssertIsRunning();

			// put the service in a runnable state
			RegisterComponent(service);			
			
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

		public void RegisterComponent(ILighthouseComponent component)
		{
			Log(LogLevel.Debug, this, $"Added component: {component}.");
			component.StatusUpdated += Service_StatusUpdated;

			// subclass specific operations
			if (component is IResourceProvider rs)
				Resources.Add(rs);

			if(component is ILighthouseService service)
				service.Initialize(this);
		}
		#endregion

		#region Auditing/Logging
		private void HandleTaskCompletion(Task task)
		{
			Log(LogLevel.Debug, this, $"App completed successfully. {ServiceThreads.FirstOrDefault(lsr => lsr.Task == task)?.Service}");
		}

		private void Service_StatusUpdated(ILighthouseLogSource owner, string status)
		{
			Log(LogLevel.Debug, owner, status);
		}

		//void RaiseStatusUpdated(string statusChangeMessage)
		//{
		//	StatusUpdated?.Invoke(this, statusChangeMessage);
		//}

		public void Log(LogLevel level, ILighthouseLogSource sender, string message)
		{
			string log = $"[{DateTime.Now.ToString("HH:mm:fff")}] [{sender}]: {message}";

			// ALL messages are logged locally for now
			LogLocally(log);

			// right now these status updates are anonymous. But this is is for specific messages in the serer, not not services running within the server
			StatusUpdated?.Invoke(null, message);

			// emit the messages in the event context as well, so it can be reacted to there as well
			EventContext?.Log(BusDriver.Core.Logging.LogType.Info, message, sender is ILogSource ls ? ls : this);
		}
		#endregion

		#region Service Discovery
		public IEnumerable<LighthouseServiceRun> GetRunningServices()
			=> ServiceThreads.Where(s => s.Service.RunState > LighthouseServiceRunState.PendingStart && s.Service.RunState < LighthouseServiceRunState.PendingStop);

		public IEnumerable<T> FindServices<T>() where T : ILighthouseService
		{
			return ServiceThreads.Select(st => st.Service).OfType<T>();
		}

		public IEnumerable<T> FindRemoteServices<T>() where T : ILighthouseService
		{
			return Enumerable.Empty<T>();
		}
		#endregion

		#region Resources
		private readonly ConcurrentBag<IResourceProvider> Resources = new ConcurrentBag<IResourceProvider>();

		private void LoadProviders()
		{
			Log(LogLevel.Debug, this, "Loading server-local resources...");
			Log(LogLevel.Debug, this, $"WorkingDirectory:{WorkingDirectory}");

			// TODO: allow for a discovery of the various providers, using reflection

			// TODO: factor out how the "root" directory is found. This probably needs to be an environment config option
			// File System providers
			if (OS == OSPlatform.Windows)
				RegisterComponent(new WindowsFileSystemProvider(WorkingDirectory, this));
			else if (OS == OSPlatform.Linux)
				RegisterComponent(new UnixFileSystemProvider());

			// TODO: add network support
		}

		public IEnumerable<T> GetResourceProviders<T>()
			where T : IResourceProvider
		{
			return Resources.OfType<T>();
		}

		public IEnumerable<IFileSystemProvider> GetFileSystemProviders()
		{
			return GetResourceProviders<IFileSystemProvider>();
		}

		public IEnumerable<INetworkProvider> GetNetworkProviders()
		{
			return GetResourceProviders<INetworkProvider>();
		}
		#endregion

		#region Utils
		public DateTime GetTime()
		{
			// for now, just use local time, but this should eventually use UTC
			return DateTime.Now;
		}

		public override string ToString()
		{
			return "Lighthouse Server";
		}
		#endregion
	}
}