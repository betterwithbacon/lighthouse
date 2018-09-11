using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lighthouse.Core.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Lighthouse.Core.Configuration.Formats.YAML;

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

			return Enumerable.Empty<IServiceRepository>();
		}

		public IEnumerable<ServiceLaunchRequest> GetServiceLaunchRequests()
		{
			if (!IsInitted)
				Init();

			return null;
		}

		private void Init()
		{
			// TODO: this is a complete mess right now, as it makes A LOT OF assumptions, about what files will be used and from where ,but this is just step 1.
			var completeConfigFilePath = ConfigFilePath ?? Path.Combine(LighthouseContainer.WorkingDirectory, DEFAULT_CONFIG_FILENAME);
			configData = LighthouseContainer.GetFileSystemProviders().FirstOrDefault()?.ReadStringFromFileSystem(completeConfigFilePath) ?? "";
			if(configData != null)
			{
				var deserializer = new DeserializerBuilder()
					.WithNamingConvention(new CamelCaseNamingConvention())
					.Build();
				
				var config = deserializer.Deserialize<LighthouseYamlConfig>(configData);
				config
			}
			else
			{
				throw new ApplicationException("");
			}
		}

		private bool IsInitted => configData != null;
	}
}
