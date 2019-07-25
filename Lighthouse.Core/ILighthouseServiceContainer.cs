using Lighthouse.Core.Configuration.Providers;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Events;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Management;
using Lighthouse.Core.Storage;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Lighthouse.Core
{
	public interface ILighthouseServiceContainer
	{
		string ServerName { get; }

		/// <summary>
		/// A generic way to log messages. These messages should be emitted as events in the Context as well as any internal logging.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="sender"></param>
		/// <param name="message"></param>
		void Log(LogLevel level, LogType logType, object sender, string message = null, Exception exception = null, bool emitEvent = true);

		/// <summary>
		/// The current container time
		/// </summary>
		/// <returns></returns>
		DateTime GetNow();

		/// <summary>
		/// Exposes low-level server local resources, such as the disk, network, or specific hardware devices.		
		/// This should NOT be used as a substitute for higher level abstractions such as <see cref="Warehouse.Core.IWarehouse"/>.
		/// These resources should be wrapped by lighthouse services and exposed to other applications.
		/// </summary>		
		/// <returns></returns>
		IEnumerable<IFileSystemProvider> GetFileSystemProviders();

        TResponse HandleRequest<TRequest, TResponse>(TRequest storageRequest)
            where TRequest : class;

        /// <summary>
        /// Exposes low-level server local resources, such as the disk, network, or specific hardware devices.		
        /// This should NOT be used as a substitute for higher level abstractions such as <see cref="Warehouse.Core.IWarehouse"/>.
        /// These resources should be wrapped by lighthouse services and exposed to other applications.
        /// </summary>
        /// <returns></returns>
        IEnumerable<INetworkProvider> GetNetworkProviders();

		ILighthouseServiceContainerConnection Connect(Uri uri);

		void RegisterResourceProvider(IResourceProvider resourceProvider);

		/// <summary>
		/// The working directory of the Lighthouse runtime
		/// </summary>
		string WorkingDirectory { get; }

        /// <summary>
        /// Will perform the action context-attached non-blocking error-resistant environment, meaning they can potentially receive/emit events if the worker threads have subscriber
        /// The work may be queued up, so items performed in parallel should simply be added sequentially.
        /// </summary>
        /// <param name="actions"></param>
        Task Do(Action<ILighthouseServiceContainer> action, string logMessage = "");
        
        IConfigurationProvider GetConfigurationProvider();

        /// <summary>
        /// Add a scheduled action attached to the object that created them
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="taskToPerform"></param>
        /// <param name="minuteFrequency"></param>
        Task AddScheduledAction(ILighthouseService owner, Action<DateTime> taskToPerform, int minuteFrequency = 1, string scheduleName = null);

        // removes all scheduled actions created by this owner
        void RemoveScheduledActions(ILighthouseService owner, string scheduleName = null);

        void RegisterEventProducer(IEventProducer eventSource);

		void RegisterEventConsumer<TEvent>(IEventConsumer eventConsumer)
			where TEvent : IEvent;

		Task EmitEvent(IEvent ev, object source = null);

		IEnumerable<IEvent> GetAllReceivedEvents(PointInTime since = null);
		
		void RegisterRemotePeer(ILighthouseServiceContainerConnection connection);

		/// <summary>
		/// A common warehouse, where abstracted storage interfaces are exposed. Otherwarehouses might be attached to this container, but this is a container guaranteed to exist, and expose local resources.
		/// </summary>
		IWarehouse Warehouse { get; }

		IEnumerable<ILighthouseServiceContainerConnection> FindServers();

		void Start();
		Task Stop();

		//void AddServiceRepository(IServiceRepository serviceRepository);

		//void AddServiceLaunchRequest(ServiceLaunchRequest launchRequest, bool persist = false, bool autoStart = false);

		LighthouseServerStatus GetStatus();
        void Launch(ILighthouseService service);
    }
}