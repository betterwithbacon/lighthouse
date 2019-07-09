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
	public interface IFileContentProvider
	{
		string GetContent();
	}

	public class MemoryContentProvider : IFileContentProvider
	{
		private readonly string content;
		public MemoryContentProvider(string content)
		{
			this.content = content;
		}

		public string GetContent()
		{
			return content;
		}
	}

	public class FileSystemContentProvider : IFileContentProvider
	{
		readonly ILighthouseServiceContainer lighthouseContainer;
		readonly string filePath;

		public string GetContent()
		{
			return lighthouseContainer.GetFileSystemProviders().FirstOrDefault()?.ReadStringFromFileSystem(filePath) ?? "";
		}

		public FileSystemContentProvider(ILighthouseServiceContainer lighthouseContainer, string filePath)
		{
			this.lighthouseContainer = lighthouseContainer;
			this.filePath = filePath;
		}

		public override string ToString()
		{
			return filePath;
		}
	}

	public class FileBasedConfigProvider<T> : IConfigurationProvider
		where T : LighthouseYamlBaseConfig
	{
		private readonly string ConfigFilePath;
		public ILighthouseServiceContainer LighthouseContainer { get; }
		public string Name { get; private set; }
		public LighthouseConfigType ConfigType { get; private set; }
		private IFileContentProvider FileContentProvider { get; }
		protected T Config { get; set; }
		
		//Dictionary<LighthouseConfigType, Type> ConfigTypeTypeMappings = new Dictionary<LighthouseConfigType, Type>
		//{
		//	{ LighthouseConfigType.App, typeof(LighthouseYamlAppConfig)}
		//};

		public FileBasedConfigProvider(ILighthouseServiceContainer lighthouseContainer, IFileContentProvider fileContentProvider)
		{
			LighthouseContainer = lighthouseContainer ?? throw new ArgumentNullException(nameof(lighthouseContainer));			
			FileContentProvider = fileContentProvider ?? throw new ArgumentNullException(nameof(fileContentProvider));
		}

		public FileBasedConfigProvider(ILighthouseServiceContainer lighthouseContainer, string configFilePath = null)
		{
			LighthouseContainer = lighthouseContainer ?? throw new ArgumentNullException(nameof(lighthouseContainer));
			ConfigFilePath = configFilePath ?? Path.Combine(lighthouseContainer.WorkingDirectory, LighthouseYamlBaseConfig.DEFAULT_CONFIG_FILENAME); //  cache this off
			FileContentProvider = new FileSystemContentProvider(lighthouseContainer, configFilePath);			
		}

		public void Load()
		{
			var data = FileContentProvider.GetContent();

			// TODO: this is a complete mess right now, as it makes A LOT OF assumptions, about what files will be used and from where ,but this is just step 1.
			if (data != null)
			{
				var deserializer = new DeserializerBuilder()
					.WithNamingConvention(new CamelCaseNamingConvention())
					.Build();

				Config = deserializer.Deserialize<T>(data);

				// Process the common elements of a config file
				Name = Config.Name;
				ConfigType = Enum.TryParse<LighthouseConfigType>(Config.ConfigType, out var configType) ? configType : LighthouseConfigType.Component; // if we don't know the config type, make it component

				// do the deep parsing
				LoadTypeSpecificConfig(Config);
			}
			else
			{
				throw new ApplicationException($"No data was present in configuration input: {FileContentProvider}");
			}
		}

		protected virtual void LoadTypeSpecificConfig(T typeSpecificConfigData)
		{
		}

		public void Save()
		{
			var serializer = new SerializerBuilder().
				WithNamingConvention(new CamelCaseNamingConvention())
				.Build();
			var data = serializer.Serialize(Config);
			var fileSystemProvider = LighthouseContainer.GetFileSystemProviders().FirstOrDefault();
			fileSystemProvider.WriteStringToFileSystem(ConfigFilePath, data);
		}

        public T1 GetConfiguration<T1>(string identifier)
        {
            throw new NotImplementedException();
        }
    }	
}
