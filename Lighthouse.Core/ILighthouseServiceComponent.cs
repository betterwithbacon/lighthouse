using System;

namespace Lighthouse.Core
{
    public interface ILighthouseComponent
    {
		event StatusUpdatedEventHandler StatusUpdated;
	}

	public delegate void StatusUpdatedEventHandler(ILighthouseComponent owner, string status);	
}