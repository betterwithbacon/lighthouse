using FluentAssertions;
using Lighthouse.CLI.Handlers.Deployments;
using Lighthouse.Core;
using Lighthouse.Core.Configuration.Formats.Memory;
using Lighthouse.Core.Configuration.ServiceDiscovery;
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

namespace Lighthouse.CLI.Tests
{
	public class ServiceInstallationHandlerTests : IDisposable
	{
		ServiceInstallationHandler Handler { get; }
		private CliApp App { get; set; }
		AppCommand InvokedCommand { get; set; }
		private string CommandName { get; } = "testCommand";
		IList<AppCommandArgValue> ArgVals { get; } = new List<AppCommandArgValue>();
		public AppCommandExecution CommandExecution { get; private set; }
		public IAppContext AppContext { get; private set; }
		public ITestOutputHelper Output { get; }

		private List<string> ConsoleMessages = new List<string>();

		Action<string> ConsoleWriteLine;
		Func<string> ReadFromConsole = null;		
		Func<ConsoleKeyInfo> ReadKeyFromConsole = null;

		public ServiceInstallationHandlerTests(ITestOutputHelper output)
		{
			Handler = new ServiceInstallationHandler();
			ConsoleWriteLine = (text) => ConsoleMessages.Add(text);
			Output = output;
		}

		[Fact]
		public async Task Execute_WithArg_ServiceResolved()
		{	
			GivenACommand();
			WithArgValue(ServiceInstallationHandler.COMMAND_NAME, 
				ServiceInstallationHandler.Arguments.APP_NAME, "testApp");
			WithArgValue(ServiceInstallationHandler.COMMAND_NAME, 
				ServiceInstallationHandler.Arguments.TARGET_SERVER, "127.0.0.1");
			await WhenExecuteIsCalled();
			ThenConnectionIsMade();
		}

		private void ThenConnectionIsMade()
		{
			
		}

		[Fact]
		public async Task Execute_WithNoArg_FaultsOnARgument()
		{
			GivenAContext();
			GivenACommand();			
			await WhenExecuteIsCalled();
			ThenFaultRecorded(CliApp.CommandPrompts.MISSING_ARGUMENT);
		}

		private void ThenFaultRecorded(string expectedMessage)
		{
			faults.Any(s => s.Contains(expectedMessage)).Should().BeTrue();
		}

		private void GivenAContext()
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
			
			AppContext.GetResource<LighthouseServer>().Returns(serviceContainer);
			TestRepository = new TestRepository(null);
			TestRepository.ServiceDescriptors.Add(
				typeof(TestApp).ToServiceDescriptor()
			);
		}

		private readonly List<string> InvalidArguments = new List<string>();
		private readonly List<string> faults = new List<string>();
		private (bool, string) quitCall = ValueTuple.Create(false,"");

		public TestRepository TestRepository;

		private void WithArgValue(string commandName, string argName, string value)
		{
			var command = App.AvailableCommands.FirstOrDefault(
				c => c.CommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase)) ??
				new AppCommand(commandName, App);

			ArgVals.Add(new AppCommandArgValue
			{
				Argument = new AppCommandArgument(argName, command),
				Value = value
			});
		}

		[Fact]
		public void Execute_InvalidServiceName_ThrowsException()
		{

		}

		[Fact]
		public void Execute_ValidServiceName_ServiceFound()
		{

		}

		private async Task WhenExecuteIsCalled()
		{
			//serviceContainer.FindServiceDescriptor(Arg.Any<>)

			CommandExecution = new AppCommandExecution(App, InvokedCommand, ArgVals);
			await Handler.Execute(CommandExecution, AppContext);
		}

		private void GivenACommand()
		{
			InvokedCommand = new AppCommand(CommandName, App);
		}

		public void Dispose()
		{
			foreach(var message in ConsoleMessages)
			{
				Output.WriteLine(message);
			}
		}
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
