using System;

namespace Lighthouse.Core
{
    public interface ILighthouseService : ILighthouseComponent
    {
		string Id { get; }

		// Puts the service in a runnable state. 
		//TODO: who should own the creation of the ID, it seems like, the service should own it. Is this just about reducing coding headache of the ID creation
		void Initialize(ILighthouseServiceContext context, string id);

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