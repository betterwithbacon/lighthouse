using Lighthouse.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Server
{
    public static class LighthouseLauncher
    {
        public static T BuildService<T>(string name)
            where T: ILighthouseService<ILighthouseServiceConfigurationContext>
        {
            return default(T);
        }
    }
}
