using Lighthouse.Core.Logging;
using System;

namespace Lighthouse.Core
{
    public interface ILighthouseComponent : ILighthouseLogSource
	{
		//TODO: do we still need this? All components will have access to the container, and can call log directly from there, and pass more information
		// it would seem like testing would require the container to exist anyhow, so using events seems like a double implementation
		//[Obsolete("This will likely go away soon, as it's redundant",false)]
		//event StatusUpdatedEventHandler StatusUpdated;
		
		/// <summary>
		/// This is the runtime environment that hosts this component. It provides all of the context for the component.
		/// </summary>
		ILighthouseServiceContainer Container { get; }

		/// <summary>
		/// A simple ID that will last for the lifetime of this component in memory. Use <see cref="Utils.LighthouseComponentLifetime.GenerateSessionIdentifier"/> to aid in generation.
		/// This ID is not guaranteed to be universally unique, so it should be taken into account with a scoping identifier as well.
		/// </summary>
		/// TODO: this should be done at some point, but not sure.
		//string Identifier { get; }
	}

	public delegate void StatusUpdatedEventHandler(ILighthouseComponent owner, string status);	
}