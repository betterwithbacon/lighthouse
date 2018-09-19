using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Utils
{
	public static class LighthouseComponentLifetime
	{
		public static string GenerateSessionIdentifier(object requestor)
		{
			var rawId = Guid.NewGuid().ToString();
			//return rawId.Substring(rawId.LastIndexOf('-') + 1);
			return $"{requestor.GetType().Name}-{rawId.Substring(rawId.LastIndexOf('-') + 1)}";
		}
	}
}
