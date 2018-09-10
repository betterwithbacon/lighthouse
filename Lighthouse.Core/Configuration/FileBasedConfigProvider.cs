using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lighthouse.Core.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lighthouse.Core.Configuration
{
	public class FileBasedConfigProvider : IConfigurationProvider
	{
		public ILighthouseServiceContainer LighthouseContainer { get; }
		public event StatusUpdatedEventHandler StatusUpdated;
		private string configData;
		private readonly string ConfigFilePath;
		public static readonly string DEFAULT_CONFIG_FILENAME = "lighthouse.yaml";

		public FileBasedConfigProvider(ILighthouseServiceContainer lighthouseContainer, string configFilePath = null)
		{
			LighthouseContainer = lighthouseContainer ?? throw new ArgumentNullException(nameof(lighthouseContainer));
			ConfigFilePath = configFilePath; // this canoverride the path
		}

		public IEnumerable<IServiceRepository> GetServiceRepositories()
		{
			if (!IsInitted)
				Init();

			return null;
		}

		public IEnumerable<ServiceLaunchRequest> GetServiceLaunchRequests()
		{
			if (!IsInitted)
				Init();

			return null;
		}

		private void Init()
		{
			var completeConfigFilePath = ConfigFilePath ?? Path.Combine(LighthouseContainer.WorkingDirectory, DEFAULT_CONFIG_FILENAME);
			configData = LighthouseContainer.GetFileSystemProviders().FirstOrDefault()?.ReadStringFromFileSystem(completeConfigFilePath) ?? "";
			if(configData != null)
			{
				var deserializer = new DeserializerBuilder()
					.WithNamingConvention(new CamelCaseNamingConvention())
					.Build();
				
			}
			else
			{
				throw new ApplicationException("");
			}
		}

		private bool IsInitted => configData != null;
	}
}
