using System;
using FluentAssertions;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lighthouse.Core.Tests
{
    public class ResourceProviderTests
    {
        [Fact]
        public void LoadDatabaseProvider_Loads()
        {
            var yaml = @"
appname: Lighthouse Hub
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

            var configFile = deserializer.Deserialize<ResourceProviderConfig>(yaml);

            (bool worked, string error) = ResourceFactory.TryCreate(configFile, out var resource);
            resource.Should().NotBeNull();
            worked.Should().BeTrue();
            error.Should().BeNull();
        }
    }
}
