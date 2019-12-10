using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.CLI.Tests
{
    public class CliTests
    {
        public CliTests(ITestOutputHelper output)
        {
            Output = output;
        }

        public ITestOutputHelper Output { get; }

        [Fact]
        public void Run_ShouldParseAsExpected()
        {
            var consoleWrites = new List<string>();
            var runner = new CommandLineRunner((log) => {
                consoleWrites.Add(log);
                Output.WriteLine(log);
            }, () => "no_console_reads");

            var returnCode = runner.Run("lighthouse run -what ping -where 127.0.0.1".Split(" "));

        }
    }
}
