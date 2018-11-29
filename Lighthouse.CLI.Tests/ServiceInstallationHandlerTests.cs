using FluentAssertions;
using Lighthouse.CLI.Handlers.Deployments;
using Lighthouse.Core.UI;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Lighthouse.CLI.Tests
{
	public class ServiceInstallationHandlerTests
	{
		ServiceInstallationHandler Handler { get; }
		private CliApp App { get; set; }
		AppCommand InvokedCommand { get; set; }
		private string CommandName { get; } = "testCommand";
		IList<AppCommandArgValue> ArgVals { get; } = new List<AppCommandArgValue>();
		public AppCommandExecution CommandExecution { get; private set; }
		private List<string> ConsoleMessages = new List<string>();

		Action<string> ConsoleWriteLine;
		Func<string> ReadFromConsole = null;		
		Func<ConsoleKeyInfo> ReadKeyFromConsole = null;

		public ServiceInstallationHandlerTests()
		{
			Handler = new ServiceInstallationHandler();
			ConsoleWriteLine = (text) => ConsoleMessages.Add(text);
		}

		[Fact]
		public void Execute_WithNoArg_ThrowsException()
		{
			
		}

		[Fact]
		public async Task Execute_WithArg_NoThrowsException()
		{
			GivenAnApp();
			GivenACommand();
			WithArgValue("test", "test");
			await WhenExecuteIsCalled();
			ThenFaultRecorded(CliApp.CommandPrompts.MISSING_ARGUMENT);
		}

		private void ThenFaultRecorded(string expectedMessage)
		{
			ConsoleMessages.Any(s => s.Contains(expectedMessage)).Should().BeTrue();
		}

		private void GivenAnApp()
		{
			App = new CliApp("testApp", ConsoleWriteLine, ReadFromConsole, ReadKeyFromConsole);


		}

		private void WithArgValue(string argName, string value)
		{
			ArgVals.Add(new AppCommandArgValue
			{
				Argument = new AppCommandArgument("testArgument", new AppCommand("testCommand", App)),
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
			CommandExecution = new AppCommandExecution(App, InvokedCommand, ArgVals);
			await Handler.Execute(CommandExecution);
		}

		private void GivenACommand()
		{
			InvokedCommand = new AppCommand(CommandName, App);

		}
	}
}
