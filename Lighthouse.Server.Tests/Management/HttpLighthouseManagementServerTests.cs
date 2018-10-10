using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Server.Management;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Server.Tests.Management
{
	public class HttpLighthouseManagementServerTests : LighthouseServerTests
	{
		HttpLighthouseManagementServer ManagementServer; 

		public HttpLighthouseManagementServerTests(ITestOutputHelper output) : base(output)
		{
			ManagementServer = new HttpLighthouseManagementServer();
			ManagementServer.Initialize(Container);
		}

		[Fact]
		public void Ping_OkResponse()
		{
			// start the server
			ManagementServer.Start();
			
			var uri = new UriBuilder(Uri.UriSchemeHttp, "127.0.0.1", LighthouseContainerCommunicationUtil.DEFAULT_SERVER_PORT, LighthouseContainerCommunicationUtil.Endpoints.PING).Uri;
			
			var client = new HttpClient();

			var response = client.GetAsync(uri).Result;

			response.StatusCode.Should().Be(HttpStatusCode.OK);
			// the result of a PING should be the server time/version/and name
			var serverResponse = response.Content.ReadAsStringAsync().Result;

			if (LighthouseServerStatus.TryParse(serverResponse, out var status))
			{
				// the Container IS the remote container in this case
				status.ServerName.Should().Be(Container.ServerName);
				status.ServerTime.Date.Should().Be(DateTime.Now.Date);
			}
			else
				Assert.False(true, "The server response couldn't be parsed.");
			
		}

		[Fact]
		public void FindServiceProxies_ReturnsOneMessage()
		{

		}
	}
}
