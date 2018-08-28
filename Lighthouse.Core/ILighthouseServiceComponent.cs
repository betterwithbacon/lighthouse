using System;

namespace Lighthouse.Core
{
    public interface ILighthouseComponent
    {
		event StatusUpdatedEventHandler StatusUpdated;
		
		/// <summary>
		/// This is the runtime environment that hosts this component. It provides all of the context for the component.
		/// </summary>
		ILighthouseServiceContainer LighthouseContainer { get; }
	}

	public delegate void StatusUpdatedEventHandler(ILighthouseComponent owner, string status);	
}