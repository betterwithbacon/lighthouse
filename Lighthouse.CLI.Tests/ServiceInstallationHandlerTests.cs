using FluentAssertions;
using Lighthouse.CLI.Handlers.Deployments;
using Lighthouse.Core;
using Lighthouse.Core.Configuration.Formats.Memory;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Management;
using Lighthouse.Core.Tests;
using Lighthouse.Core.UI;
using Lighthouse.Server;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Lighthouse.CLI.Handlers.Deployments.ServiceInstallationHandler;

namespace Lighthouse.CLI.Tests
{
	public class ServiceInstallationHandlerTests : IDisposable
	{
		ServiceInstallationHandler Handler { get; }
		private CliApp App { get; set; }				
		Dictionary<string,string> ArgVals { get; } = new Dictionary<string, string>();		
		public IAppContext AppContext { get; private set; }
		public ITestOutputHelper Output { get; }

		private readonly List<string> InvalidArguments = new List<string>();
		private readonly List<string> Finishes = new List<string>();		
		private readonly List<string> Faults = new List<string>();
		private readonly List<string> Logs = new List<string>();		
		public TestRepository TestRepository;
		private (bool, string) quitCall = ValueTuple.Create(false, "");

		public ServiceInstallationHandlerTests(ITestOutputHelper output)
		{
			Handler = new ServiceInstallationHandler();			
			Output = output;
		}

		#region Tests
		[Fact]
		[Trait("Tag", "ServiceInstallationHandler")]
		[Trait("Tag", "CliHandlers")]
		[Trait("Category", "Unit")]
		public async Task Execute_WithNoAppNameArg_FaultsOnARgument()
		{
			GivenAContext();					
			await WhenExecuteIsCalled();
			ThenInvalidArgumentRecorded(Arguments.APP_NAME);			
		}

		[Fact]
		[Trait("Tag", "ServiceInstallationHandler")]
		[Trait("Tag", "CliHandlers")]
		[Trait("Category", "Unit")]
		public async Task Execute_WithBothArgs_NoServiceRepo_Faults()
		{
			GivenAContext();			
			WithArgValue(Arguments.APP_NAME, "testApp");
			WithArgValue(Arguments.TARGET_SERVER, "127.0.0.1");
			await WhenExecuteIsCalled();
			ThenFaultRecorded("no services found");
		}

		[Fact]
		[Trait("Tag", "ServiceInstallationHandler")]
		[Trait("Tag", "CliHandlers")]
		[Trait("Category", "Unit")]
		public async Task Execute_AmbiguousServiceName_Faults()
		{
			GivenAContext(withFoundServices:
				new[] {
					new ServiceDescriptor{ Name ="testApp"}					
				}
			);
			WithArgValue(Arguments.APP_NAME, "testApp");
			WithArgValue(Arguments.TARGET_SERVER, "127.0.0.1");
			await WhenExecuteIsCalled();
			ThenFaultRecorded("Multiple services found");
		}

		[Fact]
		[Trait("Tag", "ServiceInstallationHandler")]
		[Trait("Tag", "CliHandlers")]
		[Trait("Category", "Unit")]
		public async Task Execute_ValidServiceName_NoTargetSpecifiedNoOtherLighthouseServices_FaultsOnNoServiceFound()
		{
			GivenAContext(withFoundServices:
				new[] {
					new ServiceDescriptor{ Name ="testApp"}								
				}
			);

			WithArgValue(Arguments.APP_NAME, "testApp");
			await WhenExecuteIsCalled();
			ThenFaultRecorded("No target");
		}

		[Fact]
		[Trait("Tag", "ServiceInstallationHandler")]
		[Trait("Tag", "CliHandlers")]
		[Trait("Category", "Unit")]
		public async Task Execute_ValidServiceName_TargetSpecified_NetworkConnectionCreated()
		{
			var response = new ManagementInterfaceResponse(true, "");
			var mockConnection = Substitute.For<ILighthouseServiceContainerConnection>();
			mockConnection.SubmitManagementRequest(Arg.Any<ServerManagementRequestType>()).Returns(
				response
			);

			GivenAContext(
				withFoundServices:
					new[] {
						new ServiceDescriptor{ Name ="testApp"}
					},
				connectionFoundOnConnect:
					mockConnection				
			);

			var uri = "http://127.0.0.1:5050";
			WithArgValue(Arguments.APP_NAME, "testApp");
			WithArgValue(Arguments.TARGET_SERVER, uri);

			await WhenExecuteIsCalled();
			ThenLogIsMade("connection made");
			ThenLogIsMade(uri);
		}

		[Fact]
		[Trait("Tag", "ServiceInstallationHandler")]
		[Trait("Tag", "CliHandlers")]
		[Trait("Category", "Unit")]
		public async Task Execute_ValidServiceName_TargetSpecified_InvalidUri()
		{
			var response = new ManagementInterfaceResponse(true, "");
			var mockConnection = Substitute.For<ILighthouseServiceContainerConnection>();
			mockConnection.SubmitManagementRequest(Arg.Any<ServerManagementRequestType>()).Returns(
				response
			);

			GivenAContext(
				withFoundServices:
					new[] {
						new ServiceDescriptor{ Name ="testApp"}
					}				
			);
			var fakeUrl = "WRONG";
			WithArgValue(Arguments.APP_NAME, "testApp");
			WithArgValue(Arguments.TARGET_SERVER, fakeUrl);

			await WhenExecuteIsCalled();
			ThenInvalidArgumentRecorded(Arguments.TARGET_SERVER);
			// Add this stuff back in
			//ThenInvalidArgumentRecorded("Invalid URI");
			//ThenInvalidArgumentRecorded(fakeUrl);
		}

		[Fact]
		[Trait("Tag", "ServiceInstallationHandler")]
		[Trait("Tag", "CliHandlers")]
		[Trait("Category", "Unit")]
		public async Task Execute_ValidServiceName_NoTargetSpecifiedNoOtherLighthouseServices_FaultsOnMultipleServersFound()
		{
			var mockConnection = Substitute.For<ILighthouseServiceContainerConnection>();
			GivenAContext(
				withFoundServices:
					new[] {
						new ServiceDescriptor{ Name ="testApp"}
					},
				withFoundServers: new[] {
					mockConnection,
					mockConnection
				}
			);

			WithArgValue(
				Arguments.APP_NAME, "testApp");
			await WhenExecuteIsCalled();
			ThenFaultRecorded("Multiple Lighthouse servers found");
		}

		[Fact]
		[Trait("Tag", "ServiceInstallationHandler")]
		[Trait("Tag", "CliHandlers")]
		[Trait("Category", "Unit")]
		public async Task Execute_ValidServiceName_NoTargetSpecified_SingleLighthouseServiceAvailable_ManagementRequestSent_NoResponse()
		{
			var mockConnection = Substitute.For<ILighthouseServiceContainerConnection>();			
			GivenAContext(
				withFoundServices:
					new[] {
						new ServiceDescriptor{ Name ="testApp"}
					},
				withFoundServers: new[] {
					mockConnection
				}
			);

			WithArgValue(
				Arguments.APP_NAME, "testApp");
			await WhenExecuteIsCalled();
			ThenFaultRecorded("No response");
		}

		[Fact]
		[Trait("Tag", "ServiceInstallationHandler")]
		[Trait("Tag", "CliHandlers")]
		[Trait("Category", "Unit")]
		public async Task Execute_ValidServiceName_NoTargetSpecified_SingleLighthouseServiceAvailable_ManagementRequestSent()
		{
			var response = new ManagementInterfaceResponse(true, "");
			var mockConnection = Substitute.For<ILighthouseServiceContainerConnection>();
			mockConnection.SubmitManagementRequest(Arg.Any<ServerManagementRequestType>()).Returns(
				response
			);

			GivenAContext(
				withFoundServices:
					new[] {
						new ServiceDescriptor{ Name ="testApp"}
					},
				withFoundServers: new[] {
					mockConnection
				}
			);

			WithArgValue(
				Arguments.APP_NAME, "testApp");
			await WhenExecuteIsCalled();
			ThenFinishRecorded("Service installed");
		}

		[Fact]
		[Trait("Tag", "ServiceInstallationHandler")]
		[Trait("Tag", "CliHandlers")]
		[Trait("Category", "Unit")]
		public async Task Execute_ValidServiceName_NoTargetSpecified_SingleLighthouseServiceAvailable_ManagementRequestSent_RequestFailed()
		{
			var failureMessage = Guid.NewGuid().ToString();
			var response = new ManagementInterfaceResponse(false, failureMessage);
			var mockConnection = Substitute.For<ILighthouseServiceContainerConnection>();
			mockConnection.SubmitManagementRequest(Arg.Any<ServerManagementRequestType>()).Returns(
				response
			);

			GivenAContext(
				withFoundServices:
					new[] {
						new ServiceDescriptor{ Name ="testApp"}
					},
				withFoundServers: new[] {
					mockConnection
				}
			);

			WithArgValue(
				Arguments.APP_NAME, "testApp");
			await WhenExecuteIsCalled();
			ThenFaultRecorded("Installation failed");
			ThenFaultRecorded(failureMessage);
		}
		#endregion

		#region Assertions
		private void ThenInvalidArgumentRecorded(string missingArgument)
		{
			InvalidArguments.Any(s => s.Contains(missingArgument)).Should().BeTrue();
		}

		private void ThenFaultRecorded(string expectedMessage)
		{
			Faults.Any(s => s.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
		}

		private void ThenFinishRecorded(string finishMessage)
		{
			Finishes.Any(s => s.Contains(finishMessage, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
		}

		private void ThenLogIsMade(string logMessage)
		{
			Logs.Any(s => s.Contains(logMessage, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
		}
		#endregion

		#region Arrange
		private void GivenAContext(
			IList<ILighthouseServiceDescriptor> withFoundServices = null,
			IList<ILighthouseServiceContainerConnection> withFoundServers= null,
			ILighthouseServiceContainerConnection connectionFoundOnConnect = null
			)
		{
			AppContext = Substitute.For<IAppContext>();
			AppContext.Fault(Arg.Do<string>(message => Faults.Add(message)));
			AppContext.Quit(
				Arg.Do<bool>(isFatal => quitCall.Item1 = isFatal),
				Arg.Do<string>(message => quitCall.Item2 = message)
			);

			AppContext.InvalidArgument(
				Arg.Do<string>(invalidArgName => InvalidArguments.Add(invalidArgName)), 
				Arg.Any<string>()
			);

			AppContext.Finish(Arg.Do<string>(
				finishMessage => Finishes.Add(finishMessage)
			));

			AppContext.Log(Arg.Do<string>(
				log => Logs.Add(log)
			));

			var serviceContainer = Substitute.For<ILighthouseServiceContainer>();
			
			if(withFoundServices != null)
			{
				serviceContainer.FindServiceDescriptor(Arg.Any<string>()).Returns(withFoundServices);
			}

			if(withFoundServers != null)
			{
				serviceContainer.FindServers().Returns(withFoundServers);
			}

			if (connectionFoundOnConnect != null)
			{
				serviceContainer.Connect(Arg.Any<Uri>()).Returns(connectionFoundOnConnect);
			}

			AppContext.GetResource<ILighthouseServiceContainer>().Returns(serviceContainer);
			TestRepository = new TestRepository(null);
			TestRepository.ServiceDescriptors.Add(
				typeof(TestApp).ToServiceDescriptor()
			);
		}

		private void WithArgValue(string argName, string value)
		{
			ArgVals.Add(argName, value);
		}
		#endregion

		#region Actions
		private async Task WhenExecuteIsCalled()
		{
			await Handler.Handle(ArgVals, AppContext);
		}
		#endregion

		#region Heelpers
		public void Dispose()
		{
			foreach(var message in
				InvalidArguments
				.Concat(Logs)
				.Concat(Faults)				
				.Concat(Finishes))
			{
				Output.WriteLine(message);
			}
		}
		#endregion
	}

	public class TestRepository : IServiceRepository
	{
		public List<ILighthouseServiceDescriptor> ServiceDescriptors { get; private set; } = new List<ILighthouseServiceDescriptor>();
		public ILighthouseServiceContainer Container { get; set; }

		public TestRepository(ILighthouseServiceContainer container)
		{
			Container = container;			
		}

		public IEnumerable<ILighthouseServiceDescriptor> GetServiceDescriptors()
		{
			return ServiceDescriptors;
		}
	}
}


//using FluentAssertions;
//using Lighthouse.CLI.Handlers.Deployments;
//using Lighthouse.Core;
//using Lighthouse.Core.Configuration.Formats.Memory;
//using Lighthouse.Core.Configuration.ServiceDiscovery;
//using Lighthouse.Core.Tests;
//using Lighthouse.Core.UI;
//using NSubstitute;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;
//using Xunit.Abstractions;

//namespace Lighthouse.CLI.Tests
//{
//	public class ServiceInstallationHandlerTests : IDisposable
//	{
//		ServiceInstallationHandler Handler { get; }
//		private CliApp App { get; set; }
//		AppCommand InvokedCommand { get; set; }
//		private string CommandName { get; } = "testCommand";
//		IList<AppCommandArgValue> ArgVals { get; } = new List<AppCommandArgValue>();
//		public AppCommandExecution CommandExecution { get; private set; }
//		public ITestOutputHelper Output { get; }

//		private List<string> ConsoleMessages = new List<string>();

//		Action<string> ConsoleWriteLine;
//		Func<string> ReadFromConsole = null;
//		Func<ConsoleKeyInfo> ReadKeyFromConsole = null;

//		public ServiceInstallationHandlerTests(ITestOutputHelper output)
//		{
//			Handler = new ServiceInstallationHandler();
//			ConsoleWriteLine = (text) => ConsoleMessages.Add(text);
//			Output = output;
//		}

//		[Fact]
//		public async Task Execute_WithArg_ServiceResolved()
//		{
//			GivenAnApp();
//			GivenACommand();
//			WithArgValue(ServiceInstallationHandler.COMMAND_NAME,
//				ServiceInstallationHandler.Arguments.APP_NAME, "testApp");
//			WithArgValue(ServiceInstallationHandler.COMMAND_NAME,
//				ServiceInstallationHandler.Arguments.TARGET_SERVER, "127.0.0.1");
//			await WhenExecuteIsCalled();
//			ThenConnectionIsMade();
//		}

//		private void ThenConnectionIsMade()
//		{

//		}

//		[Fact]
//		public async Task Execute_WithNoArg_FaultsOnARgument()
//		{
//			GivenAnApp();
//			GivenACommand();
//			//WithArgValue(ServiceInstallationHandler.Arguments.APP_NAME, "testApp");
//			//WithArgValue(ServiceInstallationHandler.Arguments.TARGET_SERVER, "127.0.0.1");
//			await WhenExecuteIsCalled();
//			ThenFaultRecorded(CliApp.CommandPrompts.MISSING_ARGUMENT);
//		}

//		private void ThenFaultRecorded(string expectedMessage)
//		{
//			ConsoleMessages.Any(s => s.Contains(expectedMessage)).Should().BeTrue();
//		}

//		private void GivenAnApp()
//		{
//			TestRepository = new TestRepository(null);
//			TestRepository.ServiceDescriptors.Add(
//				typeof(TestApp).ToServiceDescriptor()
//			);

//			App = new LighthouseCLiApp(ConsoleWriteLine, ReadFromConsole, ReadKeyFromConsole,
//				onQuit: (isFatal, message) => { return true; },
//				onServerBuild: (server) => server.AddServiceRepository(TestRepository)
//				);
//		}

//		public TestRepository TestRepository;

//		private void WithArgValue(string commandName, string argName, string value)
//		{
//			var command = App.AvailableCommands.FirstOrDefault(
//				c => c.CommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase)) ??
//				new AppCommand(commandName, App);

//			ArgVals.Add(new AppCommandArgValue
//			{
//				Argument = new AppCommandArgument(argName, command),
//				Value = value
//			});
//		}

//		[Fact]
//		public void Execute_InvalidServiceName_ThrowsException()
//		{

//		}

//		[Fact]
//		public void Execute_ValidServiceName_ServiceFound()
//		{

//		}

//		private async Task WhenExecuteIsCalled()
//		{
//			CommandExecution = new AppCommandExecution(App, InvokedCommand, ArgVals);
//			await Handler.Execute(CommandExecution);
//		}

//		private void GivenACommand()
//		{
//			InvokedCommand = new AppCommand(CommandName, App);
//		}

//		public void Dispose()
//		{
//			foreach (var message in ConsoleMessages)
//			{
//				Output.WriteLine(message);
//			}
//		}
//	}

//	public class TestRepository : IServiceRepository
//	{
//		public List<ILighthouseServiceDescriptor> ServiceDescriptors { get; private set; } = new List<ILighthouseServiceDescriptor>();
//		public ILighthouseServiceContainer Container { get; set; }

//		public TestRepository(ILighthouseServiceContainer container)
//		{
//			Container = container;
//		}

//		public IEnumerable<ILighthouseServiceDescriptor> GetServiceDescriptors()
//		{
//			return ServiceDescriptors;
//		}
//	}
//}
