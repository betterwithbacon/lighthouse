using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lighthouse.Core.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Lighthouse.Core.Configuration.Formats.YAML;
using Lighthouse.Core.Configuration.ServiceDiscovery;

namespace Lighthouse.Core.Configuration.Providers.Local
{
	public class FileBasedConfigProvider<T> : IConfigurationProvider
		where T : LighthouseYamlBaseConfig
	{
		private readonly string ConfigFilePath;
		public ILighthouseServiceContainer LighthouseContainer { get; }
		public event StatusUpdatedEventHandler StatusUpdated;
		protected string RawConfigData { get; set; }
		public string Name { get; private set; }
		public LighthouseConfigType ConfigType { get; private set; }
		
		Dictionary<LighthouseConfigType, Type> ConfigTypeTypeMappings = new Dictionary<LighthouseConfigType, Type>
		{
			{ LighthouseConfigType.App, typeof(LighthouseYamlAppConfig)}
		};

		public FileBasedConfigProvider(ILighthouseServiceContainer lighthouseContainer, string configFilePath = null)			
		{
			LighthouseContainer = lighthouseContainer ?? throw new ArgumentNullException(nameof(lighthouseContainer));
			ConfigFilePath = configFilePath; // this can override the path

			LoadFile();			
		}

		private void LoadFile()
		{
			// TODO: this is a complete mess right now, as it makes A LOT OF assumptions, about what files will be used and from where ,but this is just step 1.
			var completeConfigFilePath = ConfigFilePath ?? Path.Combine(LighthouseContainer.WorkingDirectory, LighthouseYamlBaseConfig.DEFAULT_CONFIG_FILENAME);
			RawConfigData = LighthouseContainer.GetFileSystemProviders().FirstOrDefault()?.ReadStringFromFileSystem(completeConfigFilePath) ?? "";

			if (RawConfigData != null)
			{
				var deserializer = new DeserializerBuilder()
					.WithNamingConvention(new CamelCaseNamingConvention())
					.Build();

				var config = deserializer.Deserialize<T>(RawConfigData);

				// Process the common elements of a config file
				Name = config.Name;
				ConfigType = Enum.TryParse<LighthouseConfigType>(config.ConfigType, out var configType) ? configType : LighthouseConfigType.Component; // if we don't know the config type, make it component

				// do the deep parsing
				LoadTypeSpecificConfig(config);
			}
			else
			{
				throw new ApplicationException($"No data was present in configuration file: {ConfigFilePath ?? "<unknown path>"}");
			}
		}

		protected virtual void LoadTypeSpecificConfig(T typeSpecificConfigData)
		{
		}
	}	
}
