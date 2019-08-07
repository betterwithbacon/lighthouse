using Lighthouse.Core;
using Lighthouse.Core.Configuration.Providers;
using Lighthouse.Core.Events;
using Lighthouse.Core.Events.Queueing;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Storage;
using Lighthouse.Core.Utils;
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
		#region Fields - Server Metadata
		public string ServerName { get; private set; }
		public const string DEFAULT_APP_NAME = "Lighthouse Server";		
		public string WorkingDirectory { get; private set; } = @"C:\";
		public readonly OSPlatform OS;
        public int ServicePort { get; private set; }
        #endregion

        #region Fields - Log
        private readonly ConcurrentBag<Action<string>> LocalLoggers = new ConcurrentBag<Action<string>>();		
		#endregion

		#region Fields - Task Management
		private readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();		
		public bool IsRunning { get; private set; }
        #endregion

		#region Fields - Events		
        readonly ConcurrentBag<IEventProducer> Producers = new ConcurrentBag<IEventProducer>();
		readonly ConcurrentDictionary<Type, IList<IEventConsumer>> Consumers = new ConcurrentDictionary<Type, IList<IEventConsumer>>();
		readonly ConcurrentBag<IEvent> AllReceivedEvents = new ConcurrentBag<IEvent>();
        readonly List<ILighthouseServiceContainerConnection> RemoteContainerConnections = new List<ILighthouseServiceContainerConnection>();
		#endregion

		#region Fields - Resources
		private readonly ConcurrentBag<IResourceProvider> Resources = new ConcurrentBag<IResourceProvider>();		
		public IWarehouse Warehouse { get; } = new Warehouse();		
		#endregion

		#region Constructors
		public LighthouseServer(
			string serverName = "Lighthouse Server",
			Action<string> localLogger = null,
			string workingDirectory = null,
			Action<LighthouseServer> preLoadOperations = null)
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

            InitScheduler().GetAwaiter().GetResult();
        }
		#endregion

		#region Server Lifecycle
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
        }

		public void BindServicePort(int servicePort)
		{
			ServicePort = servicePort;
			
			// TODO: Need to restart the listening I assume?
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
		#endregion

		#region Service Launching
		public void Launch(Type serviceType)
		{
			AssertIsRunning();

			Log(LogLevel.Debug,LogType.Info,this, $"Attempting to start app: {serviceType.Name}");

			if (!(Activator.CreateInstance(serviceType) is ILighthouseService service))
				throw new ApplicationException($"App launch config doesn't represent Lighthouse app. {serviceType.AssemblyQualifiedName}");

			Launch(service);
		}

        public void Launch(ILighthouseService service)
		{
			AssertIsRunning();

            service.Initialize(this);

            // put the service in a runnable state
            RunningServices.Add(service);

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
						HandleTaskCompletion(service.ToString(), task);
					}
				}, CancellationTokenSource.Token);
		}

		public void RegisterResourceProvider(IResourceProvider resourceProvider)
		{
			Log(LogLevel.Debug, LogType.Info, this, $"Added resource: {resourceProvider}.");
			
			// subclass specific operations
			Resources.Add(resourceProvider);
		}
		#endregion

		#region Auditing/Logging
		private void HandleTaskCompletion(string serviceDescription, Task task)
		{
            Log(LogLevel.Debug, LogType.Info, this, $"{serviceDescription} completed successfully. TaskId {task.Id}");  //{RunningServices.FirstOrDefault(lsr => lsr.TaskId == task.Id)?.Service}", emitEvent:false);
		}

		private void Service_StatusUpdated(object owner, string status)
		{
			Log(LogLevel.Debug, LogType.Info, owner, status, emitEvent: false);
		}

		public void Log(LogLevel level, LogType logType, object sender, string message = null, Exception exception = null, bool emitEvent = true)
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
        public async Task EmitEvent(IEvent ev, object logSource = null)
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
			//eventProducer.Init(this);
            
			Log(LogLevel.Debug, LogType.ProducerRegistered, eventProducer);
			
			// after registered, go ahead and start the producer.
			eventProducer.Start();
		}

		public void RegisterEventConsumer<TEvent>(IEventConsumer eventConsumer)
			where TEvent : IEvent
		{
			Consumers.GetOrAdd(typeof(TEvent), new List<IEventConsumer> { eventConsumer });

			Log(LogLevel.Debug, LogType.ConsumerRegistered, sender: eventConsumer, message: eventConsumer?.ToString());
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
        private static string GetDefaultScheduleName(ILighthouseService owner, string scheduleName = null) => scheduleName ?? owner.Id + "_timer";
        private const string SchedulerSerializedObjectKeyName = "actionToPerform";

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

        public IEnumerable<ILighthouseServiceContainerConnection> FindServers()
        {
            return RemoteContainerConnections;
        }

        public TResponse HandleRequest<TRequest, TResponse>(TRequest storageRequest)
            where TRequest : class
        {
            // find request handlers
            foreach(var service in GetRunningServices())
            {
                if(service is IRequestHandler requestHandler)
                {
                    if(requestHandler.HandlesRequest<TRequest>())
                    {
                        var methods = ReflectionUtil.GetMethodsBySingleParameterType(requestHandler.GetType(), "Handle");
                        if (methods.TryGetValue(typeof(TRequest), out var method))
                        {
                            return (TResponse)method.Invoke(requestHandler, new[] { storageRequest });
                        }
                    }
                }
            }

            return default;
        }

        ConcurrentBag<ILighthouseService> RunningServices { get; set; } 
            = new ConcurrentBag<ILighthouseService>();

        private IEnumerable<ILighthouseService> GetRunningServices()
        {
            return RunningServices;
        }
    }

    public static class IRequestHandlerExtensions
    {
        public static bool HandlesRequest<TRequest>(this IRequestHandler handler)
        {
            return ((System.Reflection.TypeInfo)handler.GetType())
                                                        .ImplementedInterfaces
                                                        .Any(i => i.GenericTypeArguments.Count() == 2 && i.GenericTypeArguments[0] == typeof(TRequest));
        }
    }
}