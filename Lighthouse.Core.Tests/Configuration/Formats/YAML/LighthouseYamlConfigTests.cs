using FluentAssertions;
using Lighthouse.Core.Configuration.Formats.YAML;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lighthouse.Core.Tests.Configuration.Formats.YAML
{
	public class LighthouseYamlConfigTests
	{
		[Fact]
		public void ParseBasic_ModelParsed()
		{
			var testContent = @"---
name: 'Lighthouse Server'
version: '0.1'
maxThreadCount: '4'

service-repositories:
    -   name: local
    -   name: global
        uri: 'lighthouse:global_service_repo'

services:     
    -   name: test-app
        type: Lighthouse.Core.Apps,TestApps
    -   name: timer-app
    -   name: warehouse
    -   name: service_repo  
        type: Lighthouse.Core.Apps,ServiceRepo
        alias: global_service_repo";

			var deserializer = new DeserializerBuilder()
					.WithNamingConvention(new CamelCaseNamingConvention())
					.Build();

			var configFile = deserializer.Deserialize<LighthouseYamlConfig>(testContent);
			configFile.Should().NotBe("");
			configFile.Name.Should().Be("Lighthouse Server");
			configFile.Version.Should().Be("6.1");
			configFile.MaxThreadCount.Should().Be(4);
			configFile.Services.Count.Should().Be(3);
			configFile.Services[0].Name.Should().Be("test-app");
			configFile.Services[1].Name.Should().Be("timer-app");
			configFile.Services[2].Name.Should().Be("warehouse");

			configFile.ServiceRepositories.Count.Should().Be(2);
			configFile.ServiceRepositories[0].Name.Should().Be("local");
		}
	}
}
