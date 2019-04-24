using FluentAssertions;
using Lighthouse.Core.Configuration.Formats.Memory;
using System;
using System.IO;
using Xunit;

namespace Lighthouse.Server.Host.Tests
{
    public class LighthouseFetcherTests
    {
        [Fact]
        public void Fetch_ShouldReturnTypes()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var foundTypes = LighthouseFetcher.Fetch(null, currentDirectory, new[] { new ServiceDescriptor { Name = "test_app" } });
            foundTypes.Should().NotBeNullOrEmpty();
        }
    }
}
