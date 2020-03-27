using FluentAssertions;
using Lighthouse.Core.Database;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Lighthouse.Core.Tests.Dataabases
{
    public class InMemoryKeyValProviderTests
    {
        [Fact]
        public void TryParse_Parses()
        {
            var parsed = InMemoryKeyValProvider.TryParse("address=127.0.0.1;name=in_mem_key_val_store");
            parsed.Should().NotBeNull();
            parsed.Address.Should().Be("127.0.0.1");
            parsed.Name.Should().Be("in_mem_key_val_store");
        }
    }
}
