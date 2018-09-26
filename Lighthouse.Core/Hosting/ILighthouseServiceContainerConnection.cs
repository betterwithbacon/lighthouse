using System;
using System.Collections.Generic;
using System.Net;

namespace Lighthouse.Core.Hosting
{
	public interface ILighthouseServiceContainerConnection
	{
		bool IsConnected { get; }
		bool TryConnect();
		ILighthouseServiceContainer LighthouseServiceContainer { get; }
		IList<LighthouseServiceContainerConnectionStatus> ConnectionHistory { get; }
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