using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Deployment.Persistence
{
    public class LighthouseAppLaunchConfigDto
    {
		public string Name { get; set; }
		public Guid Id { get; set; }
		public string AssemblyPath { get; set; }
		public string NugetPackageName { get; set; }
	}
}
