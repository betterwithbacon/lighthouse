using Lighthouse.Core.Events;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Scheduling;
using System;
using System.Collections.Generic;

namespace Lighthouse.Core
{
	public interface ILighthouseServiceContainer : ILighthouseLogSource
	{
		/// <summary>
		/// A generic way to log messages. These messages should be emitted as events in the Context as well as any internal logging.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="sender"></param>
		/// <param name="message"></param>
		void Log(LogLevel level, LogType logType,  ILighthouseLogSource sender, string message = null, Exception exception = null);

		/// <summary>
		/// Finds lighthouse services that are hosted within this container.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IEnumerable<T> FindServices<T>() where T : ILighthouseService;

		/// <summary>
		/// Returns Lighthouser services that are hosted outside of this service container
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IEnumerable<T> FindRemoteServices<T>() where T : ILighthouseService;

		/// <summary>
		/// The current time, based on the lighthouse service.
		/// </summary>
		/// <returns></returns>
		DateTime GetTime();

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
		/// Will perform these actions in a context-attached, meaning they can potentially receive/emit events if the worker threads have subscriber
		/// </summary>
		/// <param name="actions"></param>
		void Do(IEnumerable<Action<ILighthouseServiceContainer>> actions);

		void AddScheduledAction(Schedule schedule, Action<DateTime> taskToPerform);

		void RegisterEventProducer(IEventProducer eventSource);

		void RegisterEventConsumer<TEvent>(IEventConsumer eventConsumer)
			where TEvent : IEvent;

		void EmitEvent(IEvent ev, ILighthouseLogSource source = null);

		IEnumerable<IEvent> GetAllReceivedEvents(PointInTime since = null);
	}
}