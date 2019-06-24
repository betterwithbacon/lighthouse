namespace Lighthouse.Core.Configuration.Formats.YAML
{
    public class LighthouseYamlAppConfig : LighthouseYamlBaseConfig
    {
        public int MaxThreadCount { get; set; }        
    }

    //public static class LighthouseYamlAppConfigExtensions
    //{
    //	public static void LoadConfigurationAppYaml(this ILighthouseServiceContainer lighthouseServiceContainer, string yamlContent)
    //	{
    //		lighthouseServiceContainer.RegisterResourceProvider(new FileBasedAppConfigProvider(lighthouseServiceContainer, new MemoryContentProvider(yamlContent)));
    //	}
    //}

}
