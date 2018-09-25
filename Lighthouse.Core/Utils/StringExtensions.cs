using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Utils
{
	public static class StringExtensions
	{
		public static string ToLogSummary(this string input, int maxLength = 25)
		{
			if (input.Length >= maxLength)
				return input.Substring(0, maxLength);
			else
				return input;
		}
	}
}
