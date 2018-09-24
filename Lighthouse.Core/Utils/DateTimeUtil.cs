using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Utils
{
	public static class DateTimeUtilExtensions
	{
		public static DateTime RemoveSeconds(this DateTime date)
		{
			return date.AddSeconds(-1 * date.Second);
		}

		public static string ToLighthouseLogString(this DateTime date)
		{
			return date.ToString("HH:mm:ss:fff");
		}
	}
}
