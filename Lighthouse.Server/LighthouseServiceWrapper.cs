using Lighthouse.Core;

namespace Lighthouse.Server
{
	internal class LighthouseServiceWrapper
	{
		private object iD;
		private ILighthouseService service;

		public LighthouseServiceWrapper(object iD, ILighthouseService service)
		{
			this.iD = iD;
			this.service = service;
		}
	}
}