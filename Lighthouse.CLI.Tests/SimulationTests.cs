using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Lighthouse.Apps.Storage.FileSync;
using Lighthouse.Core;
using Lighthouse.Core.Database;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Storage;
using Lighthouse.Core.Utils;
using Lighthouse.Server;
using NSubstitute.Extensions;
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
		public void IOT_1Master_1Storage_3Reporters()
		{
			var scenario = new Scenario(Output);

			scenario.Start(5);

			Uri resolve(LighthouseServer server) => scenario.Network.ResolveUri(server);

			var apiNode = scenario.Containers[0];
			var dbNode = scenario.Containers[1];
			var sensorNodes = scenario.Containers.Skip(2);

			// first create a timeseries write-once DB

			// register the DB with the master node 
			// ?? I'm still not sure I know what "registration" means, it seems like that defeats the entire purpose

			// start a loop where each remote node, pings some sort of API server


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
				console: (console) =>
				{
					console.Should().Contain(RemoteAppRunStatus.Succeeded);

					// check the recent server logs and expect to see this log
					// scenario.ServerLog[dbNode].Any(l => l.Contains("warehouse added in_mem_key_val")).Should().BeTrue();
				}
			);
			
			// so the database should be running now
			scenario.ActAndAssert(
				act => act.Type($"lighthouse inspect --what services --where {resolve(dbNode)}"),
				consoleMultiLine: (console) =>
				{
					console.Any(s => s.Contains("in_mem_key_val_server")).Should().BeTrue();
				}
			);
			
			// now create a database provider to communicate with the DB
			// initially I think this was done to facilitate the traditional SQL Server model 
			// where the server was a completely separate entity that required a separate comms
			// protocol. THe DB could probably do both, at least on the server it's on (which means it should 
			// announce itself like a provider
			// while also facilitating communication via providers
			scenario.ActAndAssert(
				act => act.Type($"lighthouse inspect --what services --where {resolve(dbNode)}"),
				consoleMultiLine: (console) =>
				{
					console.Any(s => s.Contains("in_mem_key_val_server")).Should().BeTrue();
				});
			
			
			
			// at the end of the task, the DB should be full with 100k records.

		}

		[Fact]
		public void Multi_Node_Event_Parsing()
		{
			// create a cluster of 3 nodes

			// the first node creates an event

			// the second node receives the event, and processes it

			// the first node creates a "local" event

			// the second node does not receive it 

		}

		[Fact]
		public void Remote_Backup_To_local()
		{
			// test this without user connections, as we already trust the other pieces work
			var scenario = new Scenario(Output);

			scenario.Start(3);

			// Create a database, it's sole job is to store the tracking data of the file sync app
			var dbNode = scenario.Containers[1];
			const string db_identifier = "in_mem_db";
			var db = new InMemoryKeyValueDatabase(db_identifier);

			dbNode.Launch(db).GetAwaiter().GetResult();

			var folder = "C:\\test";
			var vfs_source = new VirtualFileSystem();
			var max_number_of_test_files = 10;
			var createdFiles = new Dictionary<string,string>();

			for (int i = 0; i < max_number_of_test_files; i++)
			{
				var fileName = $"{folder}\\test_file_{i}.txt";
				var payload = string.Join("", Enumerable.Repeat(i, 1000));
				createdFiles.Add(fileName, payload);

				vfs_source.WriteStringToFileSystem(fileName, payload);
			}

			// this node, has access to the DB, as well as has visibility to a local file system
			var sourceNode = scenario.Containers[2];
			
			sourceNode.RegisterResource(vfs_source);
			sourceNode.RegisterResource(
				new InMemoryKeyValProvider {
					ConnectionString =
						InMemoryKeyValueDatabaseConnection.ToString(dbNode.ServerName, db.Identifier)
				}
			);

			// create cluster of 2 nodes
			var masterNode = scenario.Containers[0];
			var vfs_master = new VirtualFileSystem();
			masterNode.RegisterResource(vfs_master);

			// launch the filesync app
			var fileSyncApp = new FileSyncApp();

			// tell the file sync config WHERE to watch
			var fileSyncConfig = new FileSyncAppConfig();
			fileSyncConfig.FoldersToWatch.Add(folder);
			fileSyncConfig.SourceServer = sourceNode.ServerName;
			fileSyncConfig.TargetServer = masterNode.ServerName;
			
			// we DON"T need to tell it where to store it's status data, as it'll just store it in the warehouse
			// and the warehouse will store it in the DB automatically
			masterNode.Launch(fileSyncApp, fileSyncConfig).GetAwaiter().GetResult();

			// at this point, the minute the file sync app is launched, it will begin attempting to sync all of the data between the folders to watch, and the remote target

			// there will be 10 virtual files, so we should wait a bit, and then verify the virtual files now exist on the file system of the remote machine
			Thread.Sleep(100); // this is arbitrary

			foreach (var file in createdFiles)
			{
				vfs_master.FileExists(file.Key).Should().BeTrue();
				vfs_master.ReadStringFromFileSystem(file.Key).Should().Be(file.Value);
			}
		}

		[Fact]
		public void LargeSimulation_All_In_Memory_ScaleOut()
		{
			var scenario = new Scenario(Output);

			scenario.Start(50);

			// create a three node cluster where the first one is what the client will interoperate with, 
			// but the other 2 will actually own the resources
			var masterNode = scenario.Containers[0];
			var workerPool = scenario.Containers.Skip(1).ToList(); // every node BUT the master

			// first, create a key_value DB on the dbNode
			// this is where the data will be stored, and the work of the worker node will be stored
			// the worker node, will be completely dumb

			var workerQueueApp = new WorkerQueueApp();
			masterNode.Launch(workerQueueApp).GetAwaiter().GetResult();

			// now the master, will queue up a bunch of work..and the worker nodes, will pick up the work, and start doing the work, and sending the results to the specified key
			var random = new Random(123_321);
			var allTestCases = new Dictionary<double, double>();
			workerQueueApp.EnqueueTasks(500, () => {
				var seed = random.Next();
				var seedSquared = Math.Pow(seed, 2);
				allTestCases.Add(seed, seedSquared);
				return (seed, seedSquared);
			});

			// the master node has queued up a bunch of work, let the workers start processing it

			// start a bunch of workers
			foreach (var container in workerPool)
			{
				var workerApp = new WorkerApp();
				workerApp.WorkQueueName = "workQueue";
				workerApp.PollTimeInMS = 100;
				container.Launch(workerApp).GetAwaiter().GetResult();
			}
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

	public class WorkerTask
	{
		public double Input { get; set; }
	}

	public class WorkerQueueApp : LighthouseServiceBase
	{
		private ConcurrentQueue<WorkerTask> Tasks { get; set; } = new ConcurrentQueue<WorkerTask>();

		public void EnqueueTasks(int numberOfTasks, Func<(double taskInput, double expectedResult)> taskAndResult)
		{
			for (var i = 0; i < numberOfTasks; i++)
			{
				var newTask = new WorkerTask
				{
					Input = taskAndResult().taskInput	
				};
				Tasks.Enqueue(newTask);
			}
		}
	}

	public class WorkerApp : LighthouseServiceBase
	{
		protected override async Task OnStart()
		{
			await Task.Run(new Action(async () => {
				while(true)
				{
					var task = Container.GetWorkQueue<WorkerTask>(WorkQueueName).Dequeue(1).FirstOrDefault();
					if(task != null)
					{
						task.Run();
					}
					else
					{
						await Task.Delay(PollTimeInMS).ConfigureAwait(false);
					}
				}
			}));
		}

		public string WorkQueueName { get; set; }
		public int PollTimeInMS { get; set; } = 1000;
	}
}
