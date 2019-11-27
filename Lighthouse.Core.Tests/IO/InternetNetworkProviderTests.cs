using FluentAssertions;
using Lighthouse.Core.IO;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Core.Tests.IO
{
    //[Collection("network tests")]
	public class InternetNetworkProviderTests : LighthouseTestsBase
	{
		public InternetNetworkProviderTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task GetStringAsync_RootRoute_ReturnsValue()
		{
			var provider = new InternetNetworkProvider();

			var port = 51515;
			var uri = new UriBuilder(Uri.UriSchemeHttp, "127.0.0.1", port).Uri;
			var exepectedResponse = "EXPECTED";

			// this guy will listen for work, in a separate thread
			var server = new TestLocalhostServer($"http://localhost:{uri.Port}/", uri.ToString());

			// simple echo endpoint, at the root
			server.AddRouteResponse("/",(request) => exepectedResponse);
			server.Start();

			// now make the internet request
			var testString = await provider.GetStringAsync(uri);
			testString.Trim().Should().Be(exepectedResponse);
			Output.WriteLine($"testString:{testString}");
		}

		[Fact]
		public async Task GetStringAsync_SubRoute_ReturnsValue()
		{
			var provider = new InternetNetworkProvider();

			var port = 51516;
			var uri = new UriBuilder(Uri.UriSchemeHttp, "127.0.0.1", port,"/PING").Uri;
			var exepectedResponse = "PING";

			// this guy will listen for work, in a separate thread
			var server = new TestLocalhostServer($"http://localhost:{uri.Port}/", $"http://127.0.0.1:{uri.Port}/".ToString());

			// simple echo endpoint, at the root
			server.AddRouteResponse("/PING", (request) => exepectedResponse);
			server.Start();

			// now make the internet request
			var testString = await provider.GetStringAsync(uri);
			testString.Trim().Should().Be(exepectedResponse);
			Output.WriteLine($"testString:{testString}");
		}
	}
}
