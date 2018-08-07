using Lighthouse.Core.Deployment.Persistence;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Lighthouse.Core.Deployment
{
	public interface ILighthouseAppLocation : ILighthouseComponent
	{
		IEnumerable<LighthouseAppLaunchConfig> FindServices();
	}

	public class LighthouseFileSystemLocation : ILighthouseAppLocation
	{
		public string Directory { get; set; }

		public event StatusUpdatedEventHandler StatusUpdated;

		public IEnumerable<LighthouseAppLaunchConfig> FindServices()
		{
			var directoryInfo = new DirectoryInfo(Directory);

			if (directoryInfo.Exists)
			{
				StatusUpdated(this, $"Directory found: {Directory}");

				foreach (var file in directoryInfo.EnumerateFiles().Where(f => f.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase)))
				{
					StatusUpdated(this, $"App File found:{file.Name} ({file.FullName})");

					var data = File.ReadAllText(file.FullName);

					var configMetadata = JsonConvert.DeserializeObject<LighthouseAppLaunchConfigDto>(data);

					if (configMetadata != null)
					{
						StatusUpdated(this, $"App loaded: {configMetadata.Name} ({configMetadata.Id})");

						yield return configMetadata.ToConfig();
					}
					else
					{
						StatusUpdated(this, $"File could not be parsed.");
					}
				}
			}
			else
			{
				StatusUpdated(this, $"Directory not found: {Directory}");
				yield break;
			}
		}

		public override string ToString()
		{
			return "File System App Locator";
		}
	}

	public static class LighthouseAppLaunchConfigDtoExtensions
	{
		public static LighthouseAppLaunchConfig ToConfig(this LighthouseAppLaunchConfigDto input)
		{	
			return new LighthouseAppLaunchConfig
			{
				Id =input.Id,
				Name = input.Name,
				// for now,going to make A LOT of assumptions about these types being in files, and permissiions, loading patterns, signing of assemblies,
				// this is NOT how most apps will be deployed, but need to see a POC around this.
				Type = Assembly.LoadFile(input.AssemblyPath).GetType(input.TypeName)
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
