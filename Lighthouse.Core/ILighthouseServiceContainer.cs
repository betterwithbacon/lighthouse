using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Events;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Management;
using Lighthouse.Core.Scheduling;
using Lighthouse.Core.Storage;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Lighthouse.Core
{
	public interface ILighthouseServiceContainer : ILighthouseLogSource
	{
		string ServerName { get; }

		/// <summary>
		/// A generic way to log messages. These messages should be emitted as events in the Context as well as any internal logging.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="sender"></param>
		/// <param name="message"></param>
		void Log(LogLevel level, LogType logType,  ILighthouseLogSource sender, string message = null, Exception exception = null, bool emitEvent = true);

		/// <summary>
		/// Returns remote services from other attached contexts. Will NOT return services from this context. 
		/// Any command issued on the local object. will be forwarded to the other service, executed, and results returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		Task<IEnumerable<LighthouseServiceProxy<T>>> FindRemoteServices<T>() where T : class, ILighthouseService;

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
		/// Registers any component passed to it. This is the best way to ensure logging and resource access is availabe
		/// </summary>
		/// <param name="component"></param>
		void RegisterComponent(ILighthouseComponent component);

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

		void AddScheduledAction(Schedule schedule, Action<DateTime> taskToPerform);

		void RegisterEventProducer(IEventProducer eventSource);

		void RegisterEventConsumer<TEvent>(IEventConsumer eventConsumer)
			where TEvent : IEvent;

		Task EmitEvent(IEvent ev, ILighthouseLogSource source = null);

		IEnumerable<IEvent> GetAllReceivedEvents(PointInTime since = null);
		
		void RegisterRemotePeer(ILighthouseServiceContainerConnection connection);

		/// <summary>
		/// A common warehouse, where abstracted storage interfaces are exposed. Otherwarehouses might be attached to this container, but this is a container guaranteed to exist, and expose local resources.
		/// </summary>
		IWarehouse Warehouse { get; }




		/*
         * This is the weird part, it seems like all of thgese pieces should only be called from "outside" the service ocntainer
         */
		ManagementInterfaceResponse HandleManagementRequest(ManagementRequestType routeName, string payload);

		IEnumerable<ILighthouseServiceContainerConnection> FindServers();

		void Start();
		Task Stop();

		void AddServiceRepository(IServiceRepository serviceRepository);

		void AddServiceLaunchRequest(ServiceLaunchRequest launchRequest, bool persist = false, bool autoStart = false);

		LighthouseServerStatus GetStatus();
	}
}