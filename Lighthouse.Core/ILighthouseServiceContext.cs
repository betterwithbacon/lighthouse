using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;

namespace Lighthouse.Core
{
	public interface ILighthouseServiceContext : ILighthouseComponent
	{
		void Log(LogLevel level, ILighthouseComponent sender, string message);
		IEnumerable<T> FindServices<T>() where T : ILighthouseService;
		IEnumerable<T> FindRemoteServices<T>() where T : ILighthouseService;
		DateTime GetTime();
	}
}