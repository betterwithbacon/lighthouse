using System;
using System.Threading.Tasks;

namespace Lighthouse.Core
{
    public interface ILighthouseService
    {
		string Id { get; }

        ILighthouseServiceContainer Container { get; }

        // Puts the service in a runnable state. 		
        void Initialize(ILighthouseServiceContainer container);

		// Begins execution of the service
		Task Start();

        // Terminates the service. This is called for "graceful exits". The service might be terminated at any time if the runtime is required to.
        Task Stop();
	}
}