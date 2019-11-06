using System;
using FluentAssertions;
using Xunit;

namespace Lighthouse.Core.Tests
{
    public class ResourceProviderConfigTests
    {
        [Fact]
        public void LoadDatabaseProvider_Loads()
        {
            var config = new ResourceProviderConfig();
            config.Type = "Database";
            config.SubType = DatabaseResourceProviderConfigSubtype.SqlServer;
            
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
