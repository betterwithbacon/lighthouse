using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Server.Tests
{
    public class StatusHandlerTests
    {
        private readonly ITestOutputHelper Output;

        public StatusHandlerTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void RequestStatus_ReturnsTime()
        {
            var server1 = new LighthouseServer(serverName: "server1", localLogger: (m) => Output.WriteLine($"server1: {m}"));
            server1.Start();

            var statusHandler = new StatusRequestHandler();
            server1.Launch(statusHandler);

            var statusRequest = new StatusRequest
            {
                
            };

            StatusResponse response = server1.HandleRequest<StatusRequest,StatusResponse>(statusRequest);
            response.ServerName.Should().Be(server1.ServerName);
        }
    }
}
