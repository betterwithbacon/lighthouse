using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

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

        public static T ConvertJsonToTarget<T>(this string input, bool throwErrors = false)
        {
            T result = default;
            try
            {
                result = JsonConvert.DeserializeObject<T>(input);
                return result;
            }
            // TODO: make this less generic Exception, just catch ones that try to deserialze
            catch (Exception)
            {
                if (throwErrors)
                    throw;
            }
            return result;
        }
    }
}
