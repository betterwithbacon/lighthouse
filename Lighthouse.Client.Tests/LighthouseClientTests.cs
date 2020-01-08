using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using FluentAssertions;
using Lighthouse.Client;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Utils;
using NSubstitute;
using Xunit;

namespace Lighthouse.Core.Tests
{

    public class LighthouseClientTests
    {
        [Fact]
        public async Task MakeRequest_SendsRequest()
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            var otherContainerPort = 12_345;
            var uri = $"http://127.0.0.1:{otherContainerPort}".ToUri();

            var otherContainer = new MockLighthouseServer();
            otherContainer.Bind(otherContainerPort);
            otherContainer.OnHandleRequest<TestRequest, TestResponse>((request) => new TestResponse { Payload = ((TestRequest)request).Payload });

            // _ = otherContainer.HandleRequest<TestRequest, TestResponse>(Arg.Any<TestRequest>()).ReturnsForAnyArgs((v) => v.ArgAt<object>(0));

            var virtualNetwork = new VirtualNetwork();
            virtualNetwork.Register(otherContainer, new System.Collections.Generic.Dictionary<string, string>() { {VirtualNetwork.DesiredUriKey, uri.ToString() } } );
                
            var client = new LighthouseClient(uri, virtualNetwork);

            var testPayload = "test";
            var testRequest = new TestRequest
            {
                Payload = testPayload
            };

            var response = await client.HandleRequest<TestRequest, TestResponse>(testRequest);
            response.Payload.Should().Be(testPayload);
        }

        public class TestRequest
        {
            public object Payload { get; set; }
        }

        public class TestResponse
        {
            public object Payload { get; set; }
        }
    }
}
