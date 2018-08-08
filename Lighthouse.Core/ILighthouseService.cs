using System;

namespace Lighthouse.Core
{
    public interface ILighthouseService : ILighthouseComponent
    {
		Guid Id { get; }

		// Puts the service in a runnable state. 
		void Initialize(ILighthouseServiceContext context, Guid id);

		// Begins execution of the service
		void Start();

		// Terminates the service. This is called for "graceful exits". The service might be terminated at any time if the runtime is required to.
		void Stop();

		// What status is the service in
		LighthouseServiceRunState RunState { get; }
	}

	public enum LighthouseServiceRunState
	{
		Created,
		PendingStart,
		Running,
		PendingStop,
		Stopped,
		Disposed
	}
}