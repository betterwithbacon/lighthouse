using Lighthouse.Server;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

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
        public void Start_DiscoversFunctions()
        {
            var functionRunner = new FunctionRunner();
            Container.Launch(functionRunner);

            var functions = functionRunner.Functions;
        }
    }
}
