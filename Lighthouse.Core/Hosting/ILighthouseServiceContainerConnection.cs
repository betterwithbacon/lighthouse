using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

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
		Task<bool> TryConnect();
		
		/// <summary>
		/// All connections made TO a server container, are unidirectionally inbound, but this flag indicates if the connectee can respond BACK to the service outside of a request.
		/// </summary>
		/// <returns></returns>
		bool IsBidirectional { get; }

		ILighthouseServiceContainer RemoteContainer { get; }
		
		/// <summary>
		/// A listing of the connection history. This information can be used when negotiating which connection to use, and for diagnostic purposes.
		/// </summary>
		IList<LighthouseServiceContainerConnectionStatus> ConnectionHistory { get; }

		/// <summary>
		/// This should resolve a service running/present in the remote container.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IEnumerable<LighthouseServiceProxy<T>> FindServices<T>()
			where T : class, ILighthouseService;
	}

	public struct LighthouseServiceContainerConnectionStatus
	{
		public DateTime EffectiveDate { get; }
		public bool WasConnected { get; }
		public Exception Exception { get; }

		public LighthouseServiceContainerConnectionStatus (DateTime effectiveDate, bool wasConnected, Exception exception = null)
		{
			EffectiveDate = effectiveDate;
			WasConnected = wasConnected;
			Exception = exception;
		}
	}
}