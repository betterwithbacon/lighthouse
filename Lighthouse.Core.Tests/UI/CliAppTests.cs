using FluentAssertions;
using Lighthouse.Core.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using static Lighthouse.Core.UI.CliApp;

namespace Lighthouse.Core.Tests.UI
{
    public class CliAppTests
    {
		private readonly ITestOutputHelper Output;
		CliApp App { get; set; }
		readonly List<string> WrittenLines;
		readonly Queue<string> LinesToWrite;

		public CliAppTests(ITestOutputHelper output)
		{
			this.Output = output;
			WrittenLines = new List<string>();
			LinesToWrite = new Queue<string>();
		}

		#region Positive ArgParsing Tests
		[Fact]
		[Trait("Matching", "Yes")]
		public void ValidCommand_NoArgs_Succeeds()
		{
			BuildApp("testapp");
			App.AddCommand("command1");

			StartAppWithArguments("command1");

			// start the app, with no args, it should succeed, but write nothing but "enter any key"
			AssertLinesWritten(1, recordsToMatch: new[] { CommandPrompts.PRESS_ANY_KEY_TO_QUIT });
		}

		[Fact]
		[Trait("Matching", "Yes")]
		public void ValidCommand_ValidArg_Succeeds()
		{
			BuildApp("testapp");
			App
				.AddCommand("command1")
				.AddArgument("commandArg1");

			StartAppWithArguments("command1 commandArg1");

			// start the app, with no args, it should succeed, but write nothing but "enter any key"
			AssertLinesWritten(1, recordsToMatch: new[] { CommandPrompts.PRESS_ANY_KEY_TO_QUIT });
		}

		[Fact]
		[Trait("Matching", "Yes")]
		public void ValidCommand_ValidArgWithVal_Succeeds()
		{
			BuildApp("testapp");
			App
				.AddCommand("command1")
				.AddArgument("commandArg1");

			StartAppWithArguments("command1 commandArg1=arg1Val");

			App.SelectedCommandArgValues[0].Value.Should().Be("arg1Val");

			// start the app, with no args, it should succeed, but write nothing but "enter any key"
			AssertLinesWritten(1, recordsToMatch: new[] { CommandPrompts.PRESS_ANY_KEY_TO_QUIT });
		}

		[Fact]
		[Trait("Matching", "Yes")]
		public void ValidCommand_Valid10Args_Succeeds()
		{
			BuildApp("testapp");
			var commandName = "validCommand";
			App.AddCommand(commandName);

			var numberOfItemsToTest = 10;
			List<string> addedArgs = new List<string>(numberOfItemsToTest);

			AppCommand command = App.GetCommand(commandName);

			foreach (var arg in Enumerable.Range(0,numberOfItemsToTest))
			{
				var newArgName = $"{command}_arg{arg}";
				command.AddArgument(newArgName);
				addedArgs.Add(newArgName);
			}
			
			StartAppWithArguments($"{commandName} {string.Join(" ",addedArgs)}");

			// start the app, with no args, it should succeed, but write nothing but "enter any key"
			AssertLinesWritten(1, recordsToMatch: new[] { CommandPrompts.PRESS_ANY_KEY_TO_QUIT });
		}
		#endregion

		#region Negative Tests
		[Fact]
		[Trait("Matching", "No")]
		public void ValidCommand_Valid9Args_Invalid1Args_Fails()
		{
			BuildApp("testapp");
			var commandName = "validCommand";
			App.AddCommand(commandName);

			var numberOfItemsToTest = 5;
			List<string> addedArgs = new List<string>(numberOfItemsToTest);

			AppCommand command = App.GetCommand(commandName);

			foreach (var arg in Enumerable.Range(0, numberOfItemsToTest))
			{
				var newArgName = $"{command}_arg{arg}";
				command.AddArgument(newArgName);
				addedArgs.Add(newArgName);
			}

			addedArgs.Add("THIS_DOESNT_EXIST");

			StartAppWithArguments($"{commandName} {string.Join(" ", addedArgs)}");

			AssertLinesWritten(3, recordsToMatch: new[] { CommandPrompts.PRESS_ANY_KEY_TO_QUIT });
		}

		[Fact]
		[Trait("Matching", "No")]
		public void ValidCommand_NoArgs_InvalidMessage()
		{
			BuildApp("testapp");
			App.AddCommand("command1");

			StartAppWithArguments();

			// start the app, with no args, it should succeed, but write nothing but "enter any key"
			AssertLinesWritten(3);
		}

		[Fact]
		[Trait("Matching", "No")]
		public void ValidCommand_InvalidArg_InvalidMessage()
		{
			BuildApp("testapp");
			App.AddCommand("command1");

			StartAppWithArguments("command1 commandArg1");

			// start the app, with no args, it should succeed, but write nothing but "enter any key"
			AssertLinesWritten(3, recordsToMatch: new[] { CommandPrompts.PRESS_ANY_KEY_TO_QUIT });
		}

		[Fact]
		[Trait("Matching", "No")]
		public void InvalidCommand_NoArg_InvalidMessage()
		{
			BuildApp("testapp");
			App.AddCommand("command1");

			StartAppWithArguments("commandNOTTTTTT");

			AssertLinesWritten(3, recordsToMatch: new[] { "Invalid Command: commandNOTTTTTT", "Valid commands are: command1", CommandPrompts.PRESS_ANY_KEY_TO_QUIT });
		}
		#endregion

		#region Command Activation - Types		
		class MockAppCommandExecutor : IAppCommandExecutor
		{
			public static readonly ConcurrentBag<string> MockAppCommandExecutorArgumentsProvided = new ConcurrentBag<string>();
			public void Execute(AppCommandExecutionArguments arguments)
			{
				MockAppCommandExecutorArgumentsProvided.Add(arguments.ArgValues[0].Value);
			}
		}

		[Fact]
		[Trait("CommandActivation-TypeBased", "Succeeds")]
		public void ValidCommand_ValidArg_ValidType_Succeeds()
		{
			string commandArg = "actualArg";

			BuildApp("testapp");
			App.AddCommand<MockAppCommandExecutor>("command1")
				.AddArgument("commandArg1", true);

			StartAppWithArguments($"command1 commandArg1={commandArg}");

			MockAppCommandExecutor.MockAppCommandExecutorArgumentsProvided.Single().Should().Be(commandArg);
		}
		#endregion

		#region Command Activation - Anonymous Functions
		[Fact]
		[Trait("CommandActivation-Lambda", "Succeeds")]
		public void ValidCommand_ValidArg_ValidFunction_WasHit_Succeeds()
		{
			bool wasHit = false;
			string commandArg = "actualArg";			
			void wasHitHandler(AppCommandExecutionArguments execution)
			{
				wasHit = true;				
			};

			BuildApp("testapp");
			App.AddCommand("command1", wasHitHandler)
				.AddArgument("commandArg1", true);

			StartAppWithArguments($"command1 commandArg1={commandArg}");

			wasHit.Should().BeTrue(because:"EventHandler was not hit.");			
		}

		[Fact]
		[Trait("CommandActivation-Lambda", "Succeeds")]
		public void ValidCommand_ValidArg_ValidFunction_ArgsArePresentInExecution_Succeeds()
		{
			string commandArg = "actualArg";
			string foundArg = "";
			void wasHitHandler(AppCommandExecutionArguments execution)
			{
				foundArg = execution.ArgValues.First().Value;
			};

			BuildApp("testapp");
			App.AddCommand("command1", wasHitHandler)
				.AddArgument("commandArg1", true);

			StartAppWithArguments($"command1 commandArg1={commandArg}");

			commandArg.Should().Be(foundArg);
		}
		#endregion

		#region Field Requiredness
		[Fact]
		[Trait("Argument Requiredness", "Fails")]
		public void ValidCommand_MissingRequiredArg_Fails()
		{
			BuildApp("testapp");
			App
				.AddCommand("command1")
				.AddArgument("commandArg1", true);

			StartAppWithArguments("command1");

			// start the app, with no args, it should succeed, but write nothing but "enter any key"
			AssertLinesWritten(2, recordsToMatch: new[] { "Required argument was missing: commandArg1",CommandPrompts.PRESS_ANY_KEY_TO_QUIT });
		}

		[Fact]
		[Trait("Argument Requiredness", "Succeeds")]
		public void ValidCommand_MissingOptionalArg_Succeeds()
		{
			BuildApp("testapp");
			App
				.AddCommand("command1")
				.AddArgument("commandArg1", false);

			StartAppWithArguments("command1");

			// start the app, with no args, it should succeed, but write nothing but "enter any key"
			AssertLinesWritten(1, recordsToMatch: new[] { CommandPrompts.PRESS_ANY_KEY_TO_QUIT });
		}
		
		#endregion

		#region Assertions
		private void AssertLinesWritten(int count, Func<string, bool> lineFilter = null, IEnumerable<string> recordsToMatch = null)
		{
			var matchedLines = lineFilter == null ? WrittenLines : WrittenLines.Where(lineFilter).ToList();

			if (recordsToMatch != null)
				matchedLines.Should().Contain(recordsToMatch);
			
			matchedLines.Count().Should().Be(count);

			Output.WriteLine($"#Console App: {App.Name}");
			Output.WriteLine($"#Console Command: {App.SelectedCommand}");
			Output.WriteLine($"#Console Command Args: {string.Join(',',App.SelectedCommandArgValues)}");
		}
		#endregion

		#region App Lifecycle
		private void BuildApp(string appName)
		{
			App = new CliApp(appName, ConsoleWrite, ConsoleRead, ConsoleAnyKey);
		}

		private string ConsoleRead()
		{
			return LinesToWrite.Dequeue();
		}

		private void StartAppWithArguments(string arguments = null)
		{
			var args = new[] { App.Name };
			var commandLine = arguments == null ? args : (IList<string>)args.Concat(arguments.Split(" ")).ToList();

			Output.WriteLine($"#Console Start: {string.Join(" ", commandLine)}");

			App.Start(commandLine);
		}

		private ConsoleKeyInfo ConsoleAnyKey () => new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);
		private ConsoleKeyInfo BlockingConsoleAnyKey()
		{
			Thread.Sleep(TimeSpan.MaxValue);
			return new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);
		}

		private void ConsoleWrite(string line)
		{
			WrittenLines.Add(line);
			Output.WriteLine(line);
		}
		#endregion
	}
}
