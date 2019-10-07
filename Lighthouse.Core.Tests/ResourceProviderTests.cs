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
            var config = new ResourceProviderConfig();
            config.Type = "Database";

            (bool worked, string error) = ResourceFactory.TryCreate(config, out var resource);
            resource.Should().NotBeNull();
            worked.Should().BeTrue();
            error.Should().BeNull();
        }

        [Fact]
        public void DatabaseResourceFactory_ReturnsDatabaseResourceProvider()
        {
            var config = new ResourceProviderConfig
            {
                SubType = DatabaseResourceProviderConfigSubtype.SqlServer.ToString()
            };

            (bool worked, string error) = DatabaseResourceFactory.TryCreate(config, out var resource);

            worked.Should().BeTrue();
            resource.Should().NotBeNull();
            error.Should().BeNull();
        }
    }
}
