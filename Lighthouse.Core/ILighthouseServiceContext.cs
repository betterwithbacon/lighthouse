using Lighthouse.Core.Logging;

namespace Lighthouse.Core
{
	public interface ILighthouseServiceContext
	{
		void Log(LogLevel level, string message);
	}
}