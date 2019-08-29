using Lighthouse.Server;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

namespace Lighthouse.Apps.Functions.Tests
{
    public class FunctionRunnerTests
    {
        protected readonly List<string> Messages = new List<string>();
        protected readonly LighthouseServer Container;
        protected ITestOutputHelper Output { get; }

        public FunctionRunnerTests(ITestOutputHelper output)
        {
            Output = output;

            Container = new LighthouseServer(localLogger: (message) => {
                Messages.Add(message); Output.WriteLine(message);
            });
        }

        [Fact]
        public async Task Start_DiscoversFunctions()
        {
            var functionRunner = new FunctionRunner();
            Container.Start();
            await Container.Launch(functionRunner);
            
            var functions = functionRunner.Functions;
            functions.Count().Should().Be(1);
        }
    }
}
