using FluentAssertions;
using Lighthouse.CLI.Handlers.Deployments;
using Lighthouse.Core;
using Lighthouse.Core.Configuration.Formats.Memory;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Hosting;
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
		AppCommand InvokedCommand { get; set; }
		private string CommandName { get; } = "testCommand";
		Dictionary<string,string> ArgVals { get; } = new Dictionary<string, string>();		
		public IAppContext AppContext { get; private set; }
		public ITestOutputHelper Output { get; }

		private List<string> ConsoleMessages = new List<string>();

		Action<string> ConsoleWriteLine;
		private readonly List<string> InvalidArguments = new List<string>();
		private readonly List<string> faults = new List<string>();
		private (bool, string) quitCall = ValueTuple.Create(false, "");
		public TestRepository TestRepository;

		public ServiceInstallationHandlerTests(ITestOutputHelper output)
		{
			Handler = new ServiceInstallationHandler();
			ConsoleWriteLine = (text) => ConsoleMessages.Add(text);
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
			WithArgValue(COMMAND_NAME,
				Arguments.APP_NAME, "testApp");
			WithArgValue(COMMAND_NAME,
				Arguments.TARGET_SERVER, "127.0.0.1");
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
					new ServiceDescriptor{ Name ="testApp"},
					new ServiceDescriptor{ Name ="testApp"}
				}
			);
			WithArgValue(COMMAND_NAME,
				Arguments.APP_NAME, "testApp");
			WithArgValue(COMMAND_NAME,
				Arguments.TARGET_SERVER, "127.0.0.1");
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

			WithArgValue(COMMAND_NAME,
				Arguments.APP_NAME, "testApp");
			await WhenExecuteIsCalled();
			ThenFaultRecorded("No target");
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

			WithArgValue(COMMAND_NAME,
				Arguments.APP_NAME, "testApp");
			await WhenExecuteIsCalled();
			ThenFaultRecorded("Multiple Lighthouse servers found");
		}

		[Fact]
		[Trait("Tag", "ServiceInstallationHandler")]
		[Trait("Tag", "CliHandlers")]
		[Trait("Category", "Unit")]
		public async Task Execute_ValidServiceName_NoTargetSpecified_SingleLighthouseServiceAvailable_ManagementRequestSent()
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

			WithArgValue(COMMAND_NAME,
				Arguments.APP_NAME, "testApp");
			await WhenExecuteIsCalled();
			ThenFaultRecorded("Multiple Lighthouse servers found");
		}
		#endregion

		#region Assertions
		private void ThenInvalidArgumentRecorded(string missingArgument)
		{
			InvalidArguments.Any(s => s.Contains(missingArgument)).Should().BeTrue();
		}

		private void ThenFaultRecorded(string expectedMessage)
		{
			faults.Any(s => s.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
		}
		#endregion

		#region Arrange
		private void GivenAContext(
			IList<ILighthouseServiceDescriptor> withFoundServices = null,
			IList<ILighthouseServiceContainerConnection> withFoundServers= null)
		{
			AppContext = Substitute.For<IAppContext>();
			AppContext.Fault(Arg.Do<string>(message => faults.Add(message)));
			AppContext.Quit(
				Arg.Do<bool>(isFatal => quitCall.Item1 = isFatal),
				Arg.Do<string>(message => quitCall.Item2 = message)
			);
			AppContext.InvalidArgument(
				Arg.Do<string>(invalidArgName => InvalidArguments.Add(invalidArgName)), 
				Arg.Any<string>()
			);

			var serviceContainer = Substitute.For<ILighthouseServiceContainer>();
			
			if(withFoundServices != null)
			{
				serviceContainer.FindServiceDescriptor(Arg.Any<string>()).Returns(withFoundServices);
			}

			if(withFoundServers != null)
			{
				serviceContainer.FindServers().Returns(withFoundServers);
			}

			AppContext.GetResource<ILighthouseServiceContainer>().Returns(serviceContainer);
			TestRepository = new TestRepository(null);
			TestRepository.ServiceDescriptors.Add(
				typeof(TestApp).ToServiceDescriptor()
			);
		}

		private void WithArgValue(string commandName, string argName, string value)
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
			foreach(var message in ConsoleMessages)
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
