using Lighthouse.Core;
using Lighthouse.Core.Configuration.Providers;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Configuration.ServiceDiscovery.Local;
using Lighthouse.Core.Events;
using Lighthouse.Core.Events.Queueing;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Management;
using Lighthouse.Core.Storage;
using Lighthouse.Core.Utils;
using Lighthouse.Server.Management;
using Lighthouse.Server.Utils;
using Lighthouse.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Async;
using System.Net;
using System.Collections.Specialized;
using Quartz.Impl;
using Quartz;

namespace Lighthouse.Server
{
	public class LighthouseServer : ILighthouseServiceContainer
	{
		public IEnumerable<ILighthouseServiceContainerConnection> FindServers()
		{
			return null;
		}

		#region Fields - Server Metadata
		public string ServerName { get; private set; }
		public const string DEFAULT_APP_NAME = "Lighthouse Server";
		public string Identifier => ServerName;
		public string WorkingDirectory { get; private set; } = @"C:\";
		public readonly OSPlatform OS;
        private static string GetDefaultScheduleName(ILighthouseService owner, string scheduleName = null) => scheduleName ?? owner.Id + "_timer";
        private const string SchedulerSerializedObjectKeyName = "actionToPerform";
        #endregion

        #region Fields - Log
        private readonly ConcurrentBag<Action<string>> LocalLoggers = new ConcurrentBag<Action<string>>();		
		#endregion

		#region Fields - Task Management
		public void AddEventQueue(IWorkQueue<IEvent> eventQueue, int pollFrequencyInMilliseconds = QueueEventProducer.DEFAULT_POLLING_INTERVAL)
		{
			// wrap the queue in a queue event producer that will read of of this
			if (eventQueue != null)			
				RegisterComponent(new QueueEventProducer(eventQueue, pollFrequencyInMilliseconds));
		}

		private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
		
		public bool IsRunning { get; private set; }
        #endregion

        #region Fields - Configuration
        private IAppConfigurationProvider AppConfiguration { get; set; }
		// Local cache of ALL repositories. this will likely include more than the initial config
		private IList<IServiceRepository> ServiceRepositories { get; set; } = new List<IServiceRepository>();
		public IList<ServiceLaunchRequest> ServiceLaunchRequests { get; private set; } = new List<ServiceLaunchRequest>();		
		public int ServicePort { get; private set; }
		#endregion

		#region Fields - Events
		public IWorkQueue<IEvent> EventQueue { get; }
		private IWorkQueue<IEvent> InternalWorkQueue { get; set; }
		readonly ConcurrentBag<IEventProducer> Producers = new ConcurrentBag<IEventProducer>();
		readonly ConcurrentDictionary<Type, IList<IEventConsumer>> Consumers = new ConcurrentDictionary<Type, IList<IEventConsumer>>();
		readonly ConcurrentBag<IEvent> AllReceivedEvents = new ConcurrentBag<IEvent>();
		Timer InternalWorkQueueProcessorTimer;		
		readonly List<ILighthouseServiceContainerConnection> RemoteContainerConnections = new List<ILighthouseServiceContainerConnection>();
		#endregion

		#region Fields - Resources
		private readonly ConcurrentBag<IResourceProvider> Resources = new ConcurrentBag<IResourceProvider>();		
		public IWarehouse Warehouse { get; } = new Warehouse();
		private readonly ConcurrentBag<ILighthouseManagementInterface> ManagementInterfaces = new ConcurrentBag<ILighthouseManagementInterface>();
		private readonly Dictionary<ManagementRequestType, Type> RequestHandlerMappings = new Dictionary<ManagementRequestType, Type>();
		#endregion

		#region Constructors
		public LighthouseServer(
			string serverName = "Lighthouse Server",
			Action<string> localLogger = null,
			string workingDirectory = null,
			Action<LighthouseServer> preLoadOperations = null,
            string[] configFileData = null,
			bool enableManagementService = false)
		{
			ServerName = serverName;
			OS = RuntimeServices.GetOS();
			WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory;

			// configure base logger
			AddLocalLogger(localLogger ?? Console.WriteLine);
						
			// the server is now actually configuring itself
			Log(LogLevel.Debug, LogType.Info, this, "Lighthouse server initializing...");

			// perform some operations before the server loads it's configuration, the most likely operations are actually adding support for loading the configuration.			
			preLoadOperations?.Invoke(this);

			// laod up app specific details
			LoadAppConfiguration();

            InitScheduler().GetAwaiter().GetResult();
        }
		#endregion

		#region Server Lifecycle
		public void AddHttpManagementInterface(int port = LighthouseContainerCommunicationUtil.DEFAULT_SERVER_PORT)
		{
			// look for a conflicting interface
			if(!ManagementInterfaces.OfType<IHttpManagementInterface>().Any(mi => mi.Port == port))
			{
				var httpLighthouseManagementServer = new HttpLighthouseManagementServer(port);
				AddManagementInterface(httpLighthouseManagementServer);
			}
			else
			{
				throw new ApplicationException($"Management interface already bound to port {port}");
			}
		}

		public void AddManagementInterface(ILighthouseManagementInterface managementInterface)
		{
			if (managementInterface == null)
			{
				throw new ArgumentNullException(nameof(managementInterface));
			}

			ManagementInterfaces.Add(managementInterface);

			// if the service is running already, go aheand and start running it
			if (IsRunning)
			{
				Launch(managementInterface);
			}
		}

        public void SetServerName(string serverName)
        {
            if (IsRunning)
                throw new ApplicationException("Can not change server name, while running ");

            ServerName = serverName;
        }

		public void AddLocalLogger(Action<string> logAction)
		{
			if(logAction != null)
				LocalLoggers.Add(logAction);
		}

		public void Start()
		{			
			Log(LogLevel.Debug,LogType.Info,this, "Lighthouse server starting");

			// the service is now runnable
			IsRunning = true;

            StartScheduler().GetAwaiter().GetResult();

            // configure a producer, that will periodically read from an event stream, and emit those events within the context.			
            if (EventQueue != null)
				RegisterEventProducer(new QueueEventProducer(EventQueue, 1000));

			AddBaseConsumers();

			LaunchConfiguredServices();
        }

		private void AddBaseConsumers()
		{
			RegisterEventConsumer<ServiceInstallationEvent>(
				new ServiceInstallationEventConsumer()
			);
		}

		private void LaunchConfiguredServices()
		{
			// TODO: right now, it's one, but it COULD be more, what's that like?!

			//// reigster all of the service requests, from the config providers.
			foreach (var request in ServiceLaunchRequests)
			{
				Log(LogLevel.Debug, LogType.Info, this, $"Preparing to start {request}");
				
				// launch the service
				Launch(request);
			}
		}

		private void LoadAppConfiguration()
		{
			var allConfigs = GetResourceProviders<IAppConfigurationProvider>();

            ServicePort = LighthouseContainerCommunicationUtil.DEFAULT_SERVER_PORT;

            if (!allConfigs.Any())
			{
				// if no config, just leave, and use the "base" config
				// throw new InvalidOperationException("No  app config provider found.");
				return;
			}

			if (allConfigs.Count() > 1)
				throw new InvalidOperationException("Too many app configuration providers found. There should only be one.");

			RequestHandlerMappings.Add(ManagementRequestType.Ping, typeof(PingManagementRequestHandler));
			RequestHandlerMappings.Add(ManagementRequestType.Services, typeof(ServicesManagementRequestHandler));
			RequestHandlerMappings.Add(ManagementRequestType.ServerManagement, typeof(ServerManagementRequestHandler));

			AppConfiguration = allConfigs.Single();

			Log(LogLevel.Debug, LogType.Info, this, $"Loading config file data {AppConfiguration}");
			AppConfiguration.Load();

			foreach (var slr in AppConfiguration.GetServiceRepositories())			
				AddServiceRepository(slr);

			// manually add "local repo" 
			// this will be everything native to the service, such as install, uninstall, etc.
			AddServiceRepository(new LocalServiceRepository(this));

			foreach (var slr in AppConfiguration.GetServiceLaunchRequests().Where(s => s != null))
				AddServiceLaunchRequest(slr);
		}

		public void BindServicePort(int servicePort)
		{
			ServicePort = servicePort;
			
			// TODO: Need to restart the listening I assume?

		}

		public void AddServiceRepository(IServiceRepository serviceRepository)
		{
			Log(LogLevel.Debug, LogType.Info, this, $"Loading service launch request: {serviceRepository}");
			ServiceRepositories.Add(serviceRepository);
		}

		public void AddServiceLaunchRequest(ServiceLaunchRequest launchRequest, bool persist = false, bool autoStart = false)
		{
			Log(LogLevel.Debug, LogType.Info, this, $"Loading service launch request: {launchRequest}");
			ServiceLaunchRequests.Add(launchRequest);

			// TODO: "install" should nominally mean that this server is now capable of running this service completely disconnectede
			// however, in the future, it would be possible for a server to remotely retrieve a package from the remote store into the local container

			if(persist)
			{
				// save the current state of the configuration
				AppConfiguration.AddServiceLaunchRequest(launchRequest);
				// TODO: obviously, the problem here is ANY other changes to the app config will ALSO be persisted
				AppConfiguration.Save();
			}

			// start the service when "installed"
			if(autoStart)
			{
				Launch(launchRequest);
			}
		}

		public async Task Stop()
		{
            //// call stop on all of the services
            //RunningServices.ToList().ForEach(serviceRun => { serviceRun.Service.Stop(); });

            //int iter = 1;

            //// just do some kindness before killing the thread. Most thread terminations should be instantaneous.
            //while(GetRunningServices().Any() && iter < 3)
            //{
            //	Log(LogLevel.Debug,LogType.Info,this, $"[Stopping] Waiting for services to finish. attempt {iter}");
            //	await Task.Delay(500);
            //	iter++;
            //}

            await Scheduler.Shutdown();

            // wait some period of time, for the services to stop by themselves, then just kill the threads out right.
            CancellationTokenSource.Cancel();

			Log(LogLevel.Debug,LogType.Info,this, "[Lighthouse Server Stopped]");
            await Task.CompletedTask; //< -- temp hackj
		}

		void AssertIsRunning()
		{
			if (!IsRunning)
				throw new InvalidOperationException("Lighthouse server is not running.");	
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
			
			//if (serviceId != null)
			//{
			//	owner = RunningServices.FirstOrDefault(ls => ls.Service.Id == serviceId);
			//	owner?.Exceptions.TryAdd(e);
			//}

			Log(LogLevel.Error,LogType.Error,null,message: $"Error occurred running task: {e.Message}", exception: e);
		}

		public void LogError(Exception exception, ILighthouseLogSource source = null)
		{
			
		}
		#endregion

		#region Service Launching
		public void Launch(IEnumerable<ServiceLaunchRequest> launchRequests)
		{
			foreach(var request in launchRequests)
			{
				Launch(request);
			}
		}

		public void Launch(Type serviceType)
		{
			AssertIsRunning();

			Log(LogLevel.Debug,LogType.Info,this, $"Attempting to start app: {serviceType.Name}");

			if (!(Activator.CreateInstance(serviceType) is ILighthouseService service))
				throw new ApplicationException($"App launch config doesn't represent Lighthouse app. {serviceType.AssemblyQualifiedName}");

			Launch(service);
		}

		public void Launch(ServiceLaunchRequest launchRequest)
		{
			AssertIsRunning();

			Log(LogLevel.Debug,LogType.Info,this, $"Attempting to start service: {launchRequest.ServiceName}.");

			var validationIssues = Validate(launchRequest);
			if (validationIssues.Any())
				throw new ApplicationException($"Can't launch service {launchRequest}. Reasons: {string.Join(',',validationIssues)}");
			
			// launch based on launch type
			switch(launchRequest.LaunchType)
			{
				case ServiceLaunchRequestType.ByType:
					Launch(launchRequest.ServiceType);
					break;
				case ServiceLaunchRequestType.ByServiceName:
					Launch(ResolveService(launchRequest.ServiceName));
					break;
				case ServiceLaunchRequestType.ByTypeThenName:
					if (launchRequest.ServiceType != null)
						Launch(launchRequest.ServiceType);
					else if (string.IsNullOrEmpty(launchRequest.ServiceName))
						Launch(ResolveService(launchRequest.ServiceName));
					else
						throw new ApplicationException("Can't launch service, with no type nor name.");
					break;
				default:
					throw new ApplicationException($"Unrecognized Launch Type {launchRequest.LaunchType}");					
			}
		}

		private ILighthouseService ResolveService(string serviceName)
		{
			// iterate over all of the repos and find the name by service
			// TODO: how do we handle ambiguity
			throw new NotImplementedException();
		}

		public IEnumerable<ServiceLaunchRequestValidationResult> Validate(ServiceLaunchRequest serviceLaunchRequest)
		{
			return Enumerable.Empty<ServiceLaunchRequestValidationResult>();
		}

		public void Launch(ILighthouseService service)
		{
			AssertIsRunning();

			// put the service in a runnable state
			RegisterComponent(service);			
			
			// start it, in a separate thread, that will run the business logic for this
			Task.Run(() => service.Start(), CancellationTokenSource.Token).ContinueWith(
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
				}, CancellationTokenSource.Token);
		}

		/// <summary>
		/// Initializes a copy of this component in the current container.
		/// </summary>
		/// <param name="component"></param>
		public void RegisterComponent(ILighthouseComponent component)
		{
			Log(LogLevel.Debug,LogType.Info,this, $"Added component: {component}.");
			
			if(component is ILighthouseService service)
				service.Initialize(this);
		}

		public void RegisterResourceProvider(IResourceProvider resourceProvider)
		{
			Log(LogLevel.Debug, LogType.Info, this, $"Added resource: {resourceProvider}.");
			
			// subclass specific operations
			Resources.Add(resourceProvider);
		}
		#endregion

		#region Auditing/Logging
		private void HandleTaskCompletion(Task task)
		{
            Log(LogLevel.Debug, LogType.Info, this, $"App completed successfully. TaskId {task.Id}");  //{RunningServices.FirstOrDefault(lsr => lsr.TaskId == task.Id)?.Service}", emitEvent:false);
		}

		private void Service_StatusUpdated(ILighthouseLogSource owner, string status)
		{
			Log(LogLevel.Debug, LogType.Info, owner, status, emitEvent: false);
		}

		public void Log(LogLevel level, LogType logType, ILighthouseLogSource sender, string message = null, Exception exception = null, bool emitEvent = true)
		{
			string log = $"[{DateTime.Now.ToLighthouseLogString()}] [{sender}] [{logType}]: {message}";

			// ALL messages are logged locally for now			
			LogLocally(log);
		}

		private void LogLocally(string log)
		{
			// fire and forget		
			LocalLoggers.ParallelForEachAsync((logger) => Task.Run(() => logger(log)));
		}
		#endregion

		#region Service Discovery		
		public IEnumerable<T> FindServices<T>() where T : ILighthouseService
		{
            return null;
			//return RunningServices.Select(st => st.Service).OfType<T>();
		}

		public IEnumerable<ILighthouseServiceDescriptor> FindServiceDescriptor(string serviceName)
		{
			if(ServiceRepositories == null)
			{
				throw new ApplicationException("Service repositorioes haven't been initialized. Ensure server is started.");
			}

			foreach(var repo in ServiceRepositories)
			{
				// TODO: do some sort of name spacing
				foreach (var foundDescriptor in repo.GetServiceDescriptors().Where((descriptor) => descriptor.Name.Equals(serviceName, StringComparison.Ordinal)))
					yield return foundDescriptor;
			}
		}

		public async Task<IEnumerable<LighthouseServiceProxy<T>>> FindRemoteServices<T>()
			where T : class, ILighthouseService
		{
			// TODO: the "are these services connected needs to be done in parallel in a 
			// background thread or something, to avoid doing this every time find remote services is called
			await RemoteContainerConnections.ParallelForEachAsync((conn) => conn.TryConnect());

			var results = new ConcurrentBag<LighthouseServiceProxy<T>>();

			//TODO: copmmented out, to make debugging a bit simpler
			//await RemoteContainerConnections.Where(conn => conn.IsConnected)
			//	.ParallelForEachAsync(async (conn) =>
			//	   {
			//		   foreach (var service in await conn.FindServices<T>())
			//			   results.Add(service);
			//	   });

			foreach(var connection in RemoteContainerConnections.Where(conn => conn.IsConnected))
				foreach (var service in await connection.FindServices<T>())
					results.Add(service);

			return results;
		}
		#endregion

		#region Resources
		

		public void AddAvailableNetworkProviders()
		{
			// add a basic internet network provider, that wraps the .Net libraries
			RegisterResourceProvider(new InternetNetworkProvider(this));
		}

		public void AddAvailableFileSystemProviders()
		{
			// TODO: factor out how the "root" directory is found. This probably needs to be an environment config option
			// File System providers
			if (OS == OSPlatform.Windows)
				RegisterResourceProvider(new WindowsFileSystemProvider(WorkingDirectory, this));
			else if (OS == OSPlatform.Linux)
				RegisterResourceProvider(new UnixFileSystemProvider());
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
		public DateTime GetNow()
		{
			// for now, just use local time, but this should eventually use UTC
			return DateTime.Now;
		}

		public override string ToString()
		{
			return ServerName;
		}
		#endregion

		#region Events
		void PollForEvents()
		{
			// kick off the timer
			// TODO: the creation of the handler should be somewhere else probably
			InternalWorkQueueProcessorTimer = 
				new Timer((context) => {					
					try
					{
						var ev = InternalWorkQueue.Dequeue(1).FirstOrDefault();
						if (ev != null)
						{
							HandleEvent(ev);
						}
					}
					catch (Exception e)
					{
						LogError(e);
						throw;
					}
				}, this, 100, 1000
			);
		}

		private void AssertProducerIsReady(IEventProducer producer)
		{
			// if the containers aren't equal, this the producer's not ready.	
			if (producer.Container != this)
				throw new ApplicationException($"Producer: {producer} is not ready.");
		}

		public async Task EmitEvent(IEvent ev, ILighthouseLogSource logSource = null)
		{	
			// log the event was raised within the context
			// but don't log events emitted by the source itself
			// subscribers can listen, but no need to alert others
			if(logSource != this)
				Log(LogLevel.Debug, LogType.EventSent, logSource, ev.ToString(), emitEvent:false);

			// ALL work should be enqueued for later execution. this means, that every event received, 
			// will be heard by both the local context, and potentially propagated to other contexts
			//EventQueue.Enqueue(ev);

			await HandleEvent(ev);
		}

		private async Task HandleEvent(IEvent ev)
		{
			// handle tasks in a separate Task
			await Do((container) => {				
				AllReceivedEvents.Add(ev);				
				if (Consumers.TryGetValue(ev.GetType(), out var consumers))
				{
					foreach (var consumer in consumers)
						consumer.HandleEvent(ev);
				}
			});
		}

		public IEnumerable<IEvent> GetAllReceivedEvents(PointInTime since = null)
		{
			return AllReceivedEvents.Where(e => since == null || since <= e.EventTime).ToArray();
		}

		public void RegisterEventProducer(IEventProducer eventProducer)
		{
			// add it
			Producers.Add(eventProducer);

			// and register this as the context with the producer
			eventProducer.Init(this);

			Log(LogLevel.Debug, LogType.ProducerRegistered, eventProducer);
			
			AssertProducerIsReady(eventProducer);

			// after registered, go ahead and start the producer.
			eventProducer.Start();
		}

		public void RegisterEventConsumer<TEvent>(IEventConsumer eventConsumer)
			where TEvent : IEvent
		{
			Consumers.GetOrAdd(typeof(TEvent), new List<IEventConsumer> { eventConsumer });

			eventConsumer.Init(this);

			Log(LogLevel.Debug, LogType.ConsumerRegistered, eventConsumer);
		}

		public async Task Do(Action<ILighthouseServiceContainer> action, string logMessage = null)
		{
			if(logMessage != null)
				Log(LogLevel.Debug, LogType.Info,this, $"Performing action {logMessage ?? "<unknown>"}");

			// just run all of the tasks			
			// TOD: do all of the magic, that this method is supposed to do
			await Task.Run(
				() => action(this), CancellationTokenSource.Token)
				.ContinueWith(
					(task) => {
						// handle errors
						if (task.IsFaulted)
						{
							HandleTaskError(task.Exception.InnerException);
						}
						// no need to handle successful "do" calls, as they're expected to succeed, and also won't have any thread metadata
						//else
						//{
						//	HandleTaskCompletion(task);
						//}
					}, CancellationTokenSource.Token);
		}
		#endregion

		#region  Hosting		
		public void RegisterRemotePeer(ILighthouseServiceContainerConnection connection)
		{
			// add it to the list
			RemoteContainerConnections.Add(connection);

			Log(LogLevel.Info, LogType.Info, this, $"Adding remote lighthouse container: {connection}");
		}

		public class LighthouseServerManagementRequestHandlerContext : IManagementRequestContext
		{
			public ILighthouseServiceContainer Container { get; internal set; }
		}

		public ManagementInterfaceResponse HandleManagementRequest(ManagementRequestType requestType, string payload)
		{
			// can't handle this reuqest
			if (!RequestHandlerMappings.ContainsKey(requestType))
			{
				Log(LogLevel.Error, LogType.Error, this, $"Unexpected management request received: {requestType}");
				return null;
			}

			var handlerType = RequestHandlerMappings[requestType];

			if (!(Activator.CreateInstance(handlerType) is IManagementRequestHandler handler))
			{
				Log(LogLevel.Error, LogType.Error, this, $"Management request handler (found:[{handlerType}]) isn't of type IManagementRequestHandler.");
				return null;
			}

			try
			{
				handler.Handle(payload,
					new LighthouseServerManagementRequestHandlerContext
					{
						Container = this
					});
			}
			catch(Exception e)
			{
				return new ManagementInterfaceResponse(false, e.Message);
			}

			return ManagementInterfaceResponse.Success;					
		}

		public ILighthouseServiceContainerConnection Connect(Uri uri)
		{
			return new NetworkLighthouseServiceContainerConnection(
				this,
				IPAddress.Parse(uri.Host),
				uri.Port
			);
		}
        #endregion

        #region Scheduling
        private async Task InitScheduler()
        {
            NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            Scheduler = await factory.GetScheduler();
        }

        private IScheduler Scheduler { get; set; }

        private async Task StartScheduler()
        {
            // and start it off
            await Scheduler.Start();
        }

        public async Task AddScheduledAction(ILighthouseService owner, Action<DateTime> taskToPerform, int minuteFrequency = 1, string scheduleName = null)
        {
            scheduleName = GetDefaultScheduleName(owner, scheduleName);
            IJobDetail job = JobBuilder.Create<ScheduledActionJob>()
                .WithIdentity(scheduleName, owner.Id)
                .Build();

            job.JobDataMap.Put(SchedulerSerializedObjectKeyName, taskToPerform);

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(scheduleName, owner.Id)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(minuteFrequency)                    
                    .RepeatForever())
                .Build();

            // Tell quartz to schedule the job using our trigger
            await Scheduler.ScheduleJob(job, trigger);
        }

        public void RemoveScheduledActions(ILighthouseService owner, string scheduleName)
        {
            Scheduler.UnscheduleJob(new TriggerKey(GetDefaultScheduleName(owner, scheduleName), owner.Id));
        }

        #endregion

        public LighthouseServerStatus GetStatus()
		{
			return new LighthouseServerStatus(
				new Version(AppConfiguration?.Version ?? "0.0.0.0"),
				ServerName, 
				GetNow()
			);
		}

        private class ScheduledActionJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                var actionSerialized = context.JobDetail.JobDataMap.Get(SchedulerSerializedObjectKeyName);
                if(actionSerialized == null)
                {
                    throw new ApplicationException("The serialized action function could not be found.");
                }

                var action = (Action<DateTime>)actionSerialized;
                action.Invoke(context.FireTimeUtc.LocalDateTime);                
                return Task.CompletedTask;
            }
        }
    }
}