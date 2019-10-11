
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lighthouse.Core
{
    public static class YamlUtil
    {
        public static T ParseYaml<T>(string yaml)
        {
            var deserializer = new DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .WithNamingConvention(UnderscoredNamingConvention.Instance)                                    
                                    .Build();

            return deserializer.Deserialize<T>(yaml);
        }
    }
}
