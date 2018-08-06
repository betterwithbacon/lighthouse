using Lighthouse.Core.Deployment.Persistence;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lighthouse.Core.Deployment
{
	public interface ILighthouseAppLocation
	{
		IEnumerable<LighthouseAppLaunchConfig> FindServices();
	}

	public class LighthouseFileSystemLocation : ILighthouseAppLocation
	{
		public string Directory { get; set; }

		public IEnumerable<LighthouseAppLaunchConfig> FindServices()
		{
			var directoryInfo = new DirectoryInfo(Directory);

			if (directoryInfo.Exists)
			{
				foreach (var file in directoryInfo.EnumerateFiles().Where(f => f.Extension.Equals("json", StringComparison.OrdinalIgnoreCase)))
				{
					var data = File.ReadAllText(file.FullName);

					var configMetadata = JsonConvert.DeserializeObject<LighthouseAppLaunchConfigDto>(data);

					yield return configMetadata.ToConfig();
				}
			}
			else
			{
				yield break;
			}
		}
	}

	public static class LighthouseAppLaunchConfigDtoExtensions
	{
		public static LighthouseAppLaunchConfig ToConfig(this LighthouseAppLaunchConfigDto input)
		{
			return new LighthouseAppLaunchConfig
			{
				
			};
		}
	}
	//public class LighthouseTypeBasedLocation : ILighthouseAppLocation
	//{
	//	public string AssemblyName { get; set; }
	//	public string AssemblyPath { get; set; }
	//}

	//public class LighthouseAppRepositoryLocation : ILighthouseAppLocation
	//{
	//	public string RepositoryUri { get; set; }
	//}
}
