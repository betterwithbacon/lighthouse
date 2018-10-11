using Lighthouse.Core;

namespace Lighthouse.Server
{
	public class LighthouseServiceRemotingWrapper
	{
		public readonly string ID;
		public readonly string ServiceTypeName;
		private readonly ILighthouseService service;

		public LighthouseServiceRemotingWrapper(string ID, ILighthouseService service)
		{
			this.ID = ID;
			this.service = service;
			ServiceTypeName = service.GetType().FullName;
		}
	}
}