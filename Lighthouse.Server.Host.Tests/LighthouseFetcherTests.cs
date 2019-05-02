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
            var serviceName = "ping";

            var currentDirectory = Directory.GetCurrentDirectory();
            var foundType = LighthouseFetcher.Fetch(serviceName, null, currentDirectory);
            foundType.Should().NotBeNull();

            foundType.Name.Should().Be(serviceName);
        }
    }
}
