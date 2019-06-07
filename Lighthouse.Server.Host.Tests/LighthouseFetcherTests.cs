using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Configuration.Formats.Memory;
using Lighthouse.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Server.Host.Tests
{
    public class LighthouseFetcherTests
    {
        private readonly ITestOutputHelper output;

        public LighthouseFetcherTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Fetch_ShouldReturnTypes()
        {
            var serviceName = "ping";

            var currentDirectory = Directory.GetCurrentDirectory();
            var foundType = LighthouseFetcher.Fetch(serviceName, null, currentDirectory);
            foundType.Should().NotBeNull();

            foundType.Name.Should().Be(typeof(PingService).Name);
        }

        [Fact]
        public void Load_ShouldFindAllTypes()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            List<string> foundTypes = new List<string>();

            foreach (var type in DllTypeLoader.Load<ILighthouseService>(currentDirectory))
            {
                foundTypes.Add(type.FullName);
            }

            foundTypes.Should().NotBeEmpty();

            foreach (var name in foundTypes.OrderBy(f => f))
            {
                output.WriteLine(name);
            }
        }
    }
}
