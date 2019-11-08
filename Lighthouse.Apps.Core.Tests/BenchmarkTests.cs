using System;
using Xunit;

namespace Lighthouse.Apps.Core.Tests
{
    public class BenchmarkTests
    {
        [Fact]
        public void NodeLatencyBenchmark_Run_NoTargets_Returns_Zero()
        {
            var benchmark = new NodeLatencyBenchmark();
            var result = benchmark.Run();

            result.
        }
    }
}
