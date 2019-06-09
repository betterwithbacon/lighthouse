using Lighthouse.Core.Logging;
using System;

namespace Lighthouse.Core
{
    public interface ILighthouseComponent : ILighthouseLogSource
	{
		/// <summary>
		/// This is the runtime environment that hosts this component. It provides all of the context for the component.
		/// </summary>
		ILighthouseServiceContainer Container { get; }
	}
}