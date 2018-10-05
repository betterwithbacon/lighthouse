using System;
using System.Collections.Generic;
using System.Net;

namespace Lighthouse.Core.Hosting
{
	public interface ILighthouseServiceContainerConnection : ILighthouseComponent
	{
		// indicates if the container is still connected. it's up to the connection itself to determine "connected" whether by a ping or connection history, so this flag might be innacurate or out of date for ephemeral connections.
		bool IsConnected { get; }

		/// <summary>
		/// Attempts to connect to the service container. If the connection is unsuccessful then it should log that to the history.
		/// </summary>
		/// <returns></returns>
		bool TryConnect();
		
		/// <summary>
		/// All connections made TO a server container, are unidirectionally inbound, but this flag indicates if the connectee can respond BACK to the service outside of a request.
		/// </summary>
		/// <returns></returns>
		bool IsBidirectional { get; }

		ILighthouseServiceContainer RemoteContainer { get; }
		IList<LighthouseServiceContainerConnectionStatus> ConnectionHistory { get; }
		IEnumerable<LighthouseServiceProxy<T>> FindServices<T>()
			where T : class, ILighthouseService;
	}

	public struct LighthouseServiceContainerConnectionStatus
	{
		DateTime EffectiveDate { get; }
		bool WasConnected { get; }
		Exception Exception { get; }

		public LighthouseServiceContainerConnectionStatus (DateTime effectiveDate, bool wasConnected, Exception exception)
		{
			EffectiveDate = effectiveDate;
			WasConnected = wasConnected;
			Exception = exception;
		}
	}
}