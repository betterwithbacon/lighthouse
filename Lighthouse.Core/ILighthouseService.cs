namespace Lighthouse.Core
{
    public interface ILighthouseService
    {
		// Puts the service in a runnable state. 
		void Initialize(ILighthouseServiceContext context);

		// Begins execution of the service
		void Start();

		// Terminates the service. This is called for "graceful exits". The service might be terminated at any time if the runtime is required to.
		void Stop();
	}
}