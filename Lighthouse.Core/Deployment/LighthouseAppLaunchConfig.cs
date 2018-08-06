using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Deployment
{
    public class LighthouseAppLaunchConfig
    {
		public string Name { get; set; }
		public Guid Id { get; set; }
		public Type Type { get; set; }
	}
}
