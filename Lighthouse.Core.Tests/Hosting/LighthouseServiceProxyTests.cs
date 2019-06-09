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

		[Fact]
		public void BasicProxy_ProxiedClass_CallsProxyMethod()
		{
			var otherContainer = new LighthouseServer();
			var remoteClassToProxy = new ClassToProxy();

			// create a connection, for the purpose of this test, we're going to connect to a local container
			var connection = new CompositeLighthouseServiceContainerConnection(
				(_) => true, (_) => true, Container
			);
			
			var proxy = new LighthouseServiceProxy<ClassToProxy>(connection, remoteClassToProxy);

			proxy.Service.Should().NotBeNull();
			proxy.Service.Id.Should().NotBeEmpty(because: "the proxied object should still be set.");

			// call something on the "local" service, which should be translated to an action on the "remote" service
			proxy.Service.MethodCallCounter.Should().Be(0);
			proxy.Service.MethodToCall();
			proxy.Service.MethodCallCounter.Should().Be(1);
		}
	}

	public class ClassToProxy : ILighthouseService
	{
		public string Id { get; }
		public int MethodCallCounter { get; private set; } = 0;

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

		public void MethodToCall()
		{
			MethodCallCounter++;
		}
	}
}
