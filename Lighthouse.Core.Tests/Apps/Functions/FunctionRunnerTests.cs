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

            Container = new LighthouseServer();
            Container.AddLogger((message) =>
            {
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
            // TODO: this is a bit of a red herring, it should nominally return 0 because no functions are added, but this asserts it doesnt' fail hard
            functions.Count().Should().Be(0);
        }
    }
}
