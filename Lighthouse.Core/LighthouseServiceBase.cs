using System;
using System.Collections.Generic;

namespace Lighthouse.Core
{
    public abstract class LighthouseServiceBase : ILighthouseService
    {
		public ILighthouseServiceContext Context { get; private set; }

		public event StatusUpdatedEventHandler StatusUpdated;

		public virtual void Start()
        {
        }

		public virtual void Stop()
		{
		}

		public void Initialize(ILighthouseServiceContext context)
        {
			Context = context;
        }
	}
}
