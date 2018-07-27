using FluentAssertions;
using Lighthouse.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

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
		
		[Fact]
		public void NoArgsCausesInvalidMessage()
		{	
			BuildApp("testapp");
			App.AddCommand("command1");

			StartAppWithArguments();

			// start the app, with no args, it should succeed, but write nothing but "enter any key"
			AssertLinesWritten(2); //, (line) => line.Contains("Invalid Argument: .", "Valid arguments are: command1"));
		}

		[Fact]
		public void MatchingCommandMatchesSuccesfully()
		{
			BuildApp("testapp");
			App.AddCommand("command1");

			StartAppWithArguments("command1");

			// start the app, with no args, it should succeed, but write nothing but "enter any key"
			AssertLinesWritten(1, recordsToMatch: new[] { "Press Any Key to quit..." });
		}

		[Fact]
		public void NonmatchingCommandFailsSuccesfully()
		{
			BuildApp("testapp");
			App.AddCommand("command1");

			StartAppWithArguments("commandNOTTTTTT");

			// should fail, with 2 lines:
			//Invalid Argument: commandNOTTTTTT. 
			// Valid arguments are: command1
			AssertLinesWritten(3, recordsToMatch: new[] {"Invalid Argument: commandNOTTTTTT. ", "Valid arguments are: command1","Press Any Key to quit..." });
		}

		#region Assertions
		private void AssertLinesWritten(int count, Func<string, bool> lineFilter = null, IEnumerable<string> recordsToMatch = null)
		{
			var matchedLines = lineFilter == null ? WrittenLines : WrittenLines.Where(lineFilter).ToList();

			if (recordsToMatch != null)
				matchedLines.Should().Contain(recordsToMatch);
			
			matchedLines.Count().Should().Be(count);
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
			App.Start(new[] { App.Name, arguments });
		}

		private ConsoleKeyInfo ConsoleAnyKey () => new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);

		private void ConsoleWrite(string line)
		{
			WrittenLines.Add(line);
			Output.WriteLine(line);
		}
		#endregion
	}
}
