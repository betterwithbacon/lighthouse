using FluentAssertions;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Utils;
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
		public void BasicProxy_PropertyReturns()
		{
			//var baseClass = new ClassToProxy();

			//var id = baseClass.Id;
						
			var proxy = new LighthouseServiceProxy<ClassToProxy>();

			proxy.Service.Should().NotBeNull();
			proxy.Service.Id.Should().NotBeEmpty(because:"the proxied object should still be set.");
			Output.WriteLine($"ProxyId: {proxy.Service.Id}");
		}
	}

	public class ClassToProxy : ILighthouseService
	{
		public string Id { get; }

		public LighthouseServiceRunState RunState => LighthouseServiceRunState.Running;

		public ILighthouseServiceContainer LighthouseContainer { get; private set; }

		public ClassToProxy()
		{
			Id = LighthouseComponentLifetime.GenerateSessionIdentifier(this);
		}

		public void Initialize(ILighthouseServiceContainer context)
		{
			LighthouseContainer = context;
		}

		public void Start()
		{
			
		}

		public void Stop()
		{
			
		}
	}
}
