using Lighthouse.Core.Logging;

namespace Lighthouse.Core.Events
{
    public interface IEventProducer : ILighthouseComponent
    {
		void Init(ILighthouseServiceContainer context);

		void Start();
	}
}
