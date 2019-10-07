using System;
using FluentAssertions;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lighthouse.Core.Tests.Configuration.Formats.YAML
{
    public class YamlLoadExtensionsTests
    {
        [Fact]
        public void Load_Loaded()
        {
            true.Should().Be(false);
        }

        [Fact]
        public void Deseriealize_Yaml()
        {
            var name = "name";

            var yaml = $@"
appname: {name}
resources:
    ms-sql-server:
        type: database
        subtype: mssqlserver    
        connection_string: localhost:1433
    redis:
        type: database
        subtype: redis
        connection_string: localhost:1433
applications:
  ping:
    type_name: Lighthouse.Apps.PingService
    configuration:";

            var deserializer = new DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .Build();

            ResourceProviderConfig config = new ResourceProviderConfig();

            config.Load(yaml);
            config.Name.Should().Be("");
        }
    }
}
