using System;
using Newtonsoft.Json;

namespace Lighthouse.Core.Utils
{
    public static class ObjectExtensions
    {
        public static string ConvertToJson(this object input, bool throwErrors = false)
        {
            try
            {
                return JsonConvert.SerializeObject(input);                
            }
            // TODO: make this less generic Exception, just catch ones that try to deserialze
            catch (Exception)
            {
                if (throwErrors)
                    throw;
            }

            return string.Empty;
        }
    }
}
