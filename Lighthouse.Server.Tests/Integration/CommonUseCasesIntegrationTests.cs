using Lighthouse.Core;
using Lighthouse.Core.Configuration.Formats.Memory;
using Lighthouse.Core.Configuration.Formats.YAML;
using Lighthouse.Core.Configuration.Providers.Local;
using Lighthouse.Core.Configuration.ServiceDiscovery.Local;
using Lighthouse.Core.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
		public void LocalMachineFolderSyncToServerTest()
		{
			// clientNode server config			
			var clientConfig = @"---
name: 'Lighthouse Node'
version: '0.1'
maxThreadCount: '1'

serviceRepositories:    
    -   name: global
        uri: 'lighthouse:global_service_repo'

services:             
    -   name: folder_sync";

			// create remote environment
			//var clientNode = 
			//	LighthouseServer
			//		.Create("Client Lighthouse")
			//		.AddLocalLogger()
			//		.Build();

			var clientNode = new LighthouseServer(
				serverName: "Client Lighthouse",
				preLoadOperations: (server) => { server.LoadConfigurationAppYaml(clientConfig); }
			);
			clientNode.AddLocalLogger((message) => Output.WriteLine($"Client: {message}"));
			//clientNode.LoadConfigurationAppYaml(clientConfig);
			
			// serverNode server config			
			// this server will use the integrated Warehouse, to do the warehouse server component.
			// however, it will also provide the app config for the client to download the "foldersync" app
			var serverConfig = @"---
name: 'Lighthouse Server'
version: '0.1'
maxThreadCount: '4'

serviceRepositories:
    -   name: local

services:
    -   name: service_repo  
        type: Lighthouse.Core.Apps,ServiceRepo
        alias: global_service_repo";

			// create server environment
			var serverNode = new LighthouseServer(
				serverName: "Server Lighthouse",
				preLoadOperations: (server) => { server.LoadConfigurationAppYaml(serverConfig); }
			);
			serverNode.AddLocalLogger((message) => Output.WriteLine($"Server: {message}"));

			// for purposes of this test, all of the ocnfig data will come from here
			//serverNode.LoadConfigurationAppYaml(serverConfig);

			// start both servers
			serverNode.Start();
			clientNode.Start();

			// enform the client about the remote peer
			// The server won't necessarily be able to "connect" back, so it will be all inbound
			// this line of code will be called on service startup
			clientNode.RegisterRemotePeer(new LocalLighthouseServiceContainerConnection(serverNode, clientNode, false));

			// at this point, what we should have are:
			// a server
			//		- a warehouse running on it
			//		- a local file system, that exposes some drives
			// a client
			//		- a warehouse running on it, with access to the local file system
			//		- a "FileSync" service running on the local machine

			Thread.Sleep(100);

			Output.WriteLine("Running Client Services:");
			foreach(var service in clientNode.GetRunningServices())
			{
				Output.WriteLine($"service running: {service}");
			}

			Output.WriteLine("Running Server Services:");
			foreach (var service in serverNode.GetRunningServices())
			{
				Output.WriteLine($"service running: {service}");
			}
		}

		//[Fact]
		//public void IfThisThenThat_PersistRules_Test()
		//{
		//	//var launcher = LighthouseLauncher<LighthouseServer>.Create<LighthouseServer>();

		//	//launcher
		//	//	.AddConfig() // add a base in memory config
		//	//		.AddServiceRepository() // add a local repo
		//	//		.AddServiceLaunchRequest()
		//	//	.Build();
		//	var serverName = "IFTTT Server";
		//	var container = new LighthouseServer(
		//		serverName: serverName,
		//		localLogger: (message) => Output.WriteLine(message)
		//	);

		//	// register the service, it might not be found by the Server
		//	container.RegisterService("ifttt", typeof(IFTTT));

		//	// now begin building the container, adding services into the container, and they will start up automatically
		//	container.AddService(typeof(Warehouse));
		//	container.AddService("ifttt");

		//	// start the server
		//	container.Start();

		//	//// add a repo to store the new IFTTT service. This service will organize the resources needed to organize this functionality
		//	//var memRepo = new MemoryServiceRepository();
		//	//memRepo.AddServiceDescriptor(new MemoryServiceDescriptor() { Name="ifttt" }); // add the IFTTT service repository.
		//	//server.AddServiceRepository(new MemoryServiceRepository());

		//	//// add memRepo to app config

		//	//server.AddServiceRepository(new LocalServiceRepository()); // add all local services

		//	// this is a server that just does IfThisThenThat workflows. the idea is to simulate this particular kind of environment
		//	// the app components can then be merged with another app			


		//	// build the config
		//}

		protected class IFTTT : LighthouseServiceBase
		{
			public IFTTT()
			{
			}
		}
	}
}
