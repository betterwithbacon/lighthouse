using FluentAssertions;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Utils;
using Lighthouse.Server;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Core.Tests.Hosting
{
	public class LighthouseServiceProxyTests : LighthouseTestsBase
	{
		public LighthouseServiceProxyTests(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public void BasicProxy_ProxiedClassReturns()
		{
			var otherContainer = new LighthouseServer();

			// create a connection, for the purpose of this test, we're going to connect to a local container
			var connection = new LocalLighthouseServiceContainerConnection(Container, otherContainer, true);

			var proxy = new LighthouseServiceProxy<ClassToProxy>(connection);

			proxy.Service.Should().NotBeNull();
			proxy.Service.Id.Should().NotBeEmpty(because:"the proxied object should still be set.");
			Output.WriteLine($"ProxyId: {proxy.Service.Id}");
		}
	}

	public class ClassToProxy : ILighthouseService
	{
		public string Id { get; }

		public LighthouseServiceRunState RunState => LighthouseServiceRunState.Running;

		public ILighthouseServiceContainer Container { get; private set; }

		public ClassToProxy()
		{
			Id = LighthouseComponentLifetime.GenerateSessionIdentifier(this);
		}

		public void Initialize(ILighthouseServiceContainer context)
		{
			Container = context;
		}

		public void Start()
		{
			
		}

		public void Stop()
		{
			
		}
	}
}
