﻿using Lighthouse.Core;
using Lighthouse.Core.Deployment;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Server
{
    public static class LighthouseLauncher
    {
        public static IEnumerable<LighthouseAppLaunchConfig> FindServices(IList<ILighthouseAppLocation> locations, Action<object,string> logHandler = null)            
        {
			foreach(var location in locations)
			{
				// wire up some basic eventing
				location.StatusUpdated += (o, m) => { logHandler(o, m); };
					
				foreach (var service in location.FindServices())
					yield return service;
			}
        }

		//public static IEnumerable<LighthouseAppLaunchConfig> FindServices(LighthouseTypeBasedLocation location)
		//{
			
		//	return FindServices(location);			
		//}
		
		//public static IEnumerable<LighthouseAppLaunchConfig> FindServices(LighthouseFileSystemLocation location)
		//{

		//	return FindServices(location);
		//}

		//public static IEnumerable<LighthouseAppLaunchConfig> FindServices(LighthouseFileSystemLocation location)
		//{

		//	return FindServices(location);
		//}
	}
}