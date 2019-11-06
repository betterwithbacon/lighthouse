using System;
using FluentAssertions;
using Lighthouse.Core.Database;
using Xunit;

namespace Lighthouse.Core.Tests
{
    public class ResourceFactoryTests
    {
        [Fact]
        public void ResourceFactory_MsSqlDbResourceProvider_Created()
        {
            var config = new ResourceProviderConfig
            {
                Type = "Database",
                SubType = DatabaseResourceProviderConfigSubtype.SqlServer
            };

            (bool worked, string error) = ResourceFactory.TryCreate(config, out var resource);
            resource.Should().NotBeNull();
            worked.Should().BeTrue();
            error.Should().BeNull();
            resource.Should().BeOfType<MsSqlDbResourceProvider>();
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
