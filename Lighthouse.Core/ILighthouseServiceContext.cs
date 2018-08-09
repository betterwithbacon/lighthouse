using Lighthouse.Core.Logging;

namespace Lighthouse.Core
{
	public interface ILighthouseServiceContext : ILighthouseComponent
	{
		void Log(LogLevel level, ILighthouseComponent sender, string message);
	}
}