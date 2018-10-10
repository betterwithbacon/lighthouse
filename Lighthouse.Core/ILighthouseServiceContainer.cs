﻿using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Events;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Management;
using Lighthouse.Core.Scheduling;
using Lighthouse.Core.Storage;
using System;
using System.Collections.Generic;

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

		IEnumerable<T> FindComponent<T>() where T : ILighthouseComponent;

		/// <summary>
		/// Finds lighthouse services that are hosted within this container.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IEnumerable<T> FindServices<T>() where T : ILighthouseService;

		/// <summary>
		/// Returns remote services from other attached contexts. Will NOT return services from this context. 
		/// Any command issued on the local object. will be forwarded to the other service, executed, and results returned.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IEnumerable<LighthouseServiceProxy<T>> FindRemoteServices<T>() where T : class, ILighthouseService;

		/// <summary>
		/// Returns Lighthouser services that are hosted outside of this service container
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		//LighthouseComponentDescriptor<T> RetrieveRemoteComponent<T>(string uri = null) where T : ILighthouseComponent;

		//T FindRemoteServiceDescriptor<T>() where T : ILighthouseServiceDescriptor;

		/// <summary>
		/// The current time, based on the lighthouse service.
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

		void RegisterResourceProvider(IResourceProvider resourceProvider);

		/// <summary>
		/// Registers any component passed to it. This is the best way to ensure logging and resource access is availabe
		/// </summary>
		/// <param name="component"></param>
		void RegisterComponent(ILighthouseComponent component);

		/// <summary>
		/// An event hook for changes to the status of the lighthouse service
		/// </summary>
		event StatusUpdatedEventHandler StatusUpdated;

		

		string WorkingDirectory { get; }

		/// <summary>
		/// Will perform the action context-attached non-blocking error-resistant environment, meaning they can potentially receive/emit events if the worker threads have subscriber
		/// The work may be queued up, so items performed in parallel should simply be added sequentially.
		/// </summary>
		/// <param name="actions"></param>
		void Do(Action<ILighthouseServiceContainer> action);

		void AddScheduledAction(Schedule schedule, Action<DateTime> taskToPerform);

		void RegisterEventProducer(IEventProducer eventSource);

		void RegisterEventConsumer<TEvent>(IEventConsumer eventConsumer)
			where TEvent : IEvent;

		void EmitEvent(IEvent ev, ILighthouseLogSource source = null);

		IEnumerable<IEvent> GetAllReceivedEvents(PointInTime since = null);
		
		void RegisterRemotePeer(ILighthouseServiceContainerConnection connection);

		/// <summary>
		/// A common warehouse, where abstracted storage interfaces are exposed. Otherwarehouses might be attached to this container, but this is a container guaranteed to exist, and expose local resources.
		/// </summary>
		IWarehouse Warehouse { get; }

		/// <summary>
		/// Represents a way to introspect the state of the container for management purposes.
		/// These APIs shouldn't be used by local services for operational purposes. merely for other containers and/or internal services
		/// </summary>
		/// <param name="routeName"></param>
		/// <param name="payload"></param>
		/// <returns></returns>
		ManagementInterfaceResponse SubmitManagementRequest(ManagementRequestType routeName, string payload);
	}
}