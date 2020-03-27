using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Storage;
using Lighthouse.Core.Utils;
using Lighthouse.Server;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.CLI.Tests
{
	public class Scenario
	{
		private Scenario() { }

		public Scenario(
			ITestOutputHelper output,
			Action<string> writeDelegate = null,
			Func<string> readDelegate = null,
			VirtualNetwork network = null)
		{
			WriteDelegate = writeDelegate ?? WriteToLog;
			ReadDelegate = readDelegate ?? Console.ReadLine;
			Network = network ?? new VirtualNetwork();
			Output = output;
			TypeFactory = new TypeFactory();
			TypeFactory.Register<INetworkProvider>(() => Network);
			Runner = new CommandLineRunner((text) =>
			{
				WriteDelegate(text);
				CurrentConsole = text;
				((List<string>)ConsoleLog).Add(text); // this is SUPPER hacky....just create  backing field for the list
			}, ReadDelegate, TypeFactory);
		}

		private readonly Action<string> WriteDelegate;
		private readonly Func<string> ReadDelegate;
		public CommandLineRunner Runner { get; }
		public TypeFactory TypeFactory { get; }
		private string CurrentConsole { get; set; }
		private List<string> consoleLog { get; set; } = new List<string>();
		public IReadOnlyCollection<string> ConsoleLog => consoleLog;
		public VirtualNetwork Network { get; } = new VirtualNetwork();
		public ITestOutputHelper Output { get; }
		public List<LighthouseServer> Containers { get; } = new List<LighthouseServer>();

		public void Start(int numberOfNodes)
		{
			for (int i = 0; i < numberOfNodes; i++)
			{
				Containers.Add(AddNode());
			}
		}

		public void Type(string text)
		{
			var message = $"USER: {text}";
			WriteDelegate(message);
			((List<string>)ConsoleLog).Add(message);

			Runner.Run(text.Split(" ").Skip(1));
		}

		public string Read() => ReadDelegate();

		internal void ActAndAssert(Action<Scenario> textToEnter, Action<string> console = null, Action<string[]> consoleMultiLine = null)
		{
			textToEnter(this);

			if (consoleMultiLine != null)
			{
				consoleMultiLine(CurrentConsole.Split(Environment.NewLine));
			}
			else if (console != null)
			{
				console(CurrentConsole);
			}
			else
			{
				// no op
			}
		}

		void WriteToLog(string text)
		{
			Output.WriteLine(text);
		}

		public IDictionary<LighthouseServer, List<string>> ServerLog { get; } = new Dictionary<LighthouseServer, List<string>>();
			
		public LighthouseServer AddNode(string name = null)
		{
			var containerName = name ?? $"container{Network.Containers.Count + 1}";
			var container = new LighthouseServer(containerName);
			container.AddLogger((log) => WriteToLog($"{containerName} LOG: {log}"));
			ServerLog.Add(container, new List<string>());
			container.AddLogger((log) => ServerLog[container].Add(log));
			container.RegisterResource(Network);
			return container;
		}
	}

	public class SimulationTests
	{
		public ITestOutputHelper Output { get; }

		public SimulationTests(ITestOutputHelper output)
		{
			Output = output;
		}

		[Fact]
		public void LargeSimulation_1NodeOrchestrates_OtherPerforms()
		{
			var scenario = new Scenario(Output);

			scenario.Start(3);

			Uri resolve(LighthouseServer server) => scenario.Network.ResolveUri(server);

			// create a three node cluster where the first one is what the client will interoperate with, 
			// but the other 2 will actually own the resources
			var masterNode = scenario.Containers[0];
			var dbNode = scenario.Containers[1];
			var cpuWorkerNode = scenario.Containers[2];

			// first, create a key_value DB on the dbNode
			// this is where the data will be stored, and the work of the worker node will be stored
			// the worker node, will be completely dumb
			var dbName = "in_mem_key_val_1";
			var newDatabaseRequest = new ResourceRequest
			{
				ResourceType = ResourceProviderType.Database,
				RequestType = ResourceRequestType.Add,
				Config = new ResourceProviderConfig
				{
					Type = ResourceProviderType.Database.ToString(),
					SubType = DatabaseResourceProviderConfigSubtype.in_memory_key_value.ToString(),
					ConnectionString = dbName // this is the literal "identifier" of the database in memory.
				}
			}.ConvertToJson(true);

			// spin up a large task on the worker node, and then let it get to work, it will be writing data back to the db, as it finishes the work
			
			scenario.ActAndAssert(
				act => act.Type($"lighthouse configure --what resource --where {resolve(cpuWorkerNode)} --how {newDatabaseRequest}"),
				consoleMultiLine: (console) =>
				{
					console.Any(s => s.Contains("added")).Should().BeTrue();

					// check the recent server logs and expect to see this log
					scenario.ServerLog[cpuWorkerNode].Any(l => l.Contains("warehouse added in_mem_key_val")).Should().BeTrue();
				}
			);

			// at this point the server is "aware" of the in_mem server
			// now actually create a server
			var newServerStartup = new Dictionary<string, string>()
			{
				{ "name", dbName }
			}.ConvertToJson(true);

			scenario.ActAndAssert(
				act => act.Type($"lighthouse run --what in_mem_key_val_server --where {resolve(dbNode)} --how {newServerStartup}"),
				consoleMultiLine: (console) =>
				{
					console.Any(s => s.Contains("added")).Should().BeTrue();

					// check the recent server logs and expect to see this log
					scenario.ServerLog[dbNode].Any(l => l.Contains("warehouse added in_mem_key_val")).Should().BeTrue();
				}
			);

			// at the end of the task, the DB should be full with 100k records.

		}

		[Fact]
		public void Multi_Node_Event_Parsing()
		{
			// create a cluster of 2 nodes

			// the first node creates an event

			// the second node receives the event, and processes it

			// the first node creates a "local" event

			// the second node does not receive it 

		}

		[Fact]
		public void LargeSimulation_All_In_Memory_ScaleOut()
		{

			var scenario = new Scenario(Output);

			scenario.Start(50);

			// cvreate a three node cluster where the first one is what the client will interoperate with, 
			// but the other 2 will actually own the resources
			var masterNode = scenario.Containers[0];
			var workerPool = scenario.Containers.Skip(1); // every node BUT the master

			// first, create a key_value DB on the dbNode
			// this is where the data will be stored, and the work of the worker node will be stored
			// the worker node, will be completely dumb



		}

		[Fact]
		public void LargeSimulation_ResourceCreation_StateInspection_ConfigurationModification()
		{
			var scenario = new Scenario(Output);
			scenario.Start(3);

			// can we reach each server
			scenario.ActAndAssert(
				act => act.Type($"lighthouse inspect --where {scenario.Network.ResolveUri(scenario.Containers[0])}"),
				(console) =>
				{
					console.Should().Contain(StatusRequestHandler.GLOBAL_VERSION_NUMBER);
					console.Should().Contain(scenario.Containers[0].ServerName);
				}
			);

			scenario.ActAndAssert(
			   act => act.Type($"lighthouse inspect --where {scenario.Network.ResolveUri(scenario.Containers[1])}"),
			   (console) =>
			   {
				   console.Should().Contain(StatusRequestHandler.GLOBAL_VERSION_NUMBER);
				   console.Should().Contain(scenario.Containers[1].ServerName);
			   }
			);

			scenario.ActAndAssert(
			   act => act.Type($"lighthouse inspect --where {scenario.Network.ResolveUri(scenario.Containers[2])}"),
			   (console) =>
			   {
				   console.Should().Contain(StatusRequestHandler.GLOBAL_VERSION_NUMBER);
				   console.Should().Contain(scenario.Containers[2].ServerName);
			   }
			);

			var container3Uri = scenario.Network.ResolveUri(scenario.Containers[2]);

			// the servers are all active and responding, now see them respond to actual pings
			scenario.ActAndAssert(
				act => act.Type($"lighthouse run --what ping --where {container3Uri}"),
				(console) =>
				{
					console.Should().Contain(RemoteAppRunStatus.Succeeded);
				}
			);

			scenario.ActAndAssert(
				  act => act.Type($"lighthouse inspect --what services --where {container3Uri}"),
				 consoleMultiLine: (console) =>
				 {
					 console.Any(s => s.Contains("ping")).Should().BeTrue();
				 }
			);

			scenario.ActAndAssert(
				 act => act.Type($"lighthouse stop --what ping --where {container3Uri}"),
				(console) =>
				{
					console.Should().Contain("ping stopped");
				}
			);

			scenario.ActAndAssert(
				 act => act.Type($"lighthouse inspect --what services --where {container3Uri}"),
				consoleMultiLine: (console) =>
				{
					console.Any(s => s.Contains("ping")).Should().BeFalse();
				}
			);

			var key = "key";
			var payload = "payload";

			// var warehouseConfig = WarehouseConfig();
			// var serializedConfig = warehouseConfig.SerializeToJSON();
			var storeRequest = new WarehouseStoreRequest
			{
				Key = key,
				Value = payload
			}.ConvertToJson(true);

			// {serializedConfig}
			scenario.ActAndAssert(
				act => act.Type($"lighthouse store --what {storeRequest} --where {container3Uri}"),
				consoleMultiLine: (console) =>
				{
					console.Should().Contain("stored");
				}
			);

			var retrieveRequest = new WarehouseRetrieveRequest
			{
				Key = key
			}.ConvertToJson(true);

			scenario.ActAndAssert(
				act => act.Type($"lighthouse retrieve --what {retrieveRequest} --where {container3Uri}"),
				consoleMultiLine: (console) =>
				{
					console.Should().Equal(payload);
				}
			);

			// ask a DIFFERENT warehouse, if it has the data, and see if it'll talk to another warehouse.
			// this is probably the simplest form of distributed storage
			scenario.ActAndAssert(
				act => act.Type($"lighthouse retrieve --what {retrieveRequest} --where {container3Uri}"),
				console: (console) =>
				{
					console.Should().Contain(payload);
				}
			);

			var resourceAddRequest = new ResourceRequest
			{
				ResourceType = ResourceProviderType.Database,
				RequestType = ResourceRequestType.Add,
				Config = new ResourceProviderConfig
				{
					Type = "Database",
					SubType = "sqlserver",
					ConnectionString = "sql_connection_string"
				}
			}.ConvertToJson(true);


			List<string> localLog = new List<string>();

			// push the logger on top
			scenario.Containers[2].AddLogger(localLog.Add);

			scenario.ActAndAssert(
				act => act.Type($"lighthouse configure --what resource --where {container3Uri} --how {resourceAddRequest}"),
				consoleMultiLine: (console) =>
				{
					console.Any(s => s.Contains("added")).Should().BeTrue();
				}
			);

			// check the recent server logs and expect to tsee this log
			localLog.Any(l => l.Contains("warehouse added sql_server")).Should().BeTrue();

			// dispose of the extra logger, now that we've verified it's in the log
			scenario.Containers[2].RunPriveleged(scenario.Containers[2].Warehouse, (c) => c.Loggers.Pop());
			localLog.Clear();

			// STOP, the test

			#region Logging
			Output.WriteLine("Entire command line: ");
			foreach (var message in scenario.ConsoleLog)
				Output.WriteLine(message);
			#endregion
		}
	}
}
