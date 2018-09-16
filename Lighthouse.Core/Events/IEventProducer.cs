using Lighthouse.Core.Logging;

namespace Lighthouse.Core.Events
{
    public interface IEventProducer : ILighthouseLogSource
    {
		void Init(IEventContext context);

		void Start();

		string GetContextId();
	}
}
