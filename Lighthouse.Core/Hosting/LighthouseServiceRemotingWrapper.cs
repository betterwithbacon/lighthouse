using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Hosting
{
	public class LighthouseServiceRemotingWrapper
	{
		public string ID { get; set; }
		public string ServiceTypeName { get; set; }
		private readonly ILighthouseService service;

		public LighthouseServiceRemotingWrapper()
		{
		}

		public LighthouseServiceRemotingWrapper(string ID, ILighthouseService service)
		{
			this.ID = ID;
			this.service = service;
			ServiceTypeName = service.GetType().AssemblyQualifiedName;
		}
	}
}
