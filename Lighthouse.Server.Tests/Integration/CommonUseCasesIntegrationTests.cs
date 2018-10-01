using Lighthouse.Core;
using Lighthouse.Core.Configuration.Formats.Memory;
using Lighthouse.Core.Configuration.Providers.Local;
using Lighthouse.Core.Configuration.ServiceDiscovery.Local;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseCore;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Server.Tests.Integration
{
	public class CommonUseCasesIntegrationTests
	{
		protected readonly ITestOutputHelper Output;

		public CommonUseCasesIntegrationTests(ITestOutputHelper output)
		{
			Output = output;
		}

		[Fact]
		public void IfThisThenThat_PersistRules_Test()
		{
			//var launcher = LighthouseLauncher<LighthouseServer>.Create<LighthouseServer>();

			//launcher
			//	.AddConfig() // add a base in memory config
			//		.AddServiceRepository() // add a local repo
			//		.AddServiceLaunchRequest()
			//	.Build();
			var serverName = "IFTTT Server";
			var container = new LighthouseServer(
				serverName: serverName,
				localLogger: (message) => Output.WriteLine(message)
			);

			// register the service, it might not be found by the Server
			container.RegisterService("ifttt", typeof(IFTTT));
			
			// now begin building the container, adding services into the container, and they will start up automatically
			container.AddService(typeof(Warehouse));
			container.AddService("ifttt");
			
			

			
			// start the server
			container.Start();

			var ifttt = container.FindServices<IFTTTService>();

			//// add a repo to store the new IFTTT service. This service will organize the resources needed to organize this functionality
			//var memRepo = new MemoryServiceRepository();
			//memRepo.AddServiceDescriptor(new MemoryServiceDescriptor() { Name="ifttt" }); // add the IFTTT service repository.
			//server.AddServiceRepository(new MemoryServiceRepository());

			//// add memRepo to app config

			//server.AddServiceRepository(new LocalServiceRepository()); // add all local services

			// this is a server that just does IfThisThenThat workflows. the idea is to simulate this particular kind of environment
			// the app components can then be merged with another app			


			// build the config
		}

		protected class IFTTT : LighthouseServiceBase
		{
			public IFTTT()
			{
			}
		}
	}
}
