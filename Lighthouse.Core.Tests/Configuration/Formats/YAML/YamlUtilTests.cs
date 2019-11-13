using System;
using FluentAssertions;
using Xunit;


namespace Lighthouse.Core.Tests
{
    public class YamlLoadExtensionsTests
    {
        [Fact]
        public void Load_Loaded()
        {
            // TODO: implement
        }

        [Fact]
        public void Deserialize_2Resource_1App_Tree()
        {
            var name = "name";

            var yaml = $@"
name: {name}
resources:
    ms-sql-server:        
        type: database
        sub_type: mssqlserver    
        connection_string: localhost:1433
    redis:        
        type: database
        sub_type: redis
        connection_string: localhost:1433
applications:
  ping:
    type_name: Lighthouse.Apps.PingService
    configuration:";

            var config = YamlUtil.ParseYaml<LighthouseRunConfig>(yaml);

            config.Name.Should().Be(name);
            config.Resources.Count.Should().Be(2);
            config.Applications.Count.Should().Be(1);
        }
    }
}
