using BusDriver.Core.Events;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;

namespace Lighthouse.Core
{
	public interface ILighthouseServiceContainer : ILighthouseComponent
	{
		/// <summary>
		/// A generic way to log messages. These messages should be emitted as events in the Context as well as any internal logging.
		/// </summary>
		/// <param name="level"></param>
		/// <param name="sender"></param>
		/// <param name="message"></param>
		void Log(LogLevel level, ILighthouseComponent sender, string message);

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
		/// This is the context where app events are published and subscribed.
		/// Communication between lighthouse components can also occur using the context
		/// 
		/// </summary>
		IEventContext EventContext { get; }

		/// <summary>
		/// Exposes low-level server local resources, such as the disk, network, or specific hardware devices.		
		/// This should NOT be used as a substitute for higher level abstractions such as <see cref="Warehouse.Core.IWarehouse"/>.
		/// These resources should be wrapped by lighthouse services and exposed to other applications.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IEnumerable<IFileSystemProvider> GetFileSystemProviders();
	}
}