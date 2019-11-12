using System;
using FluentAssertions;
using Lighthouse.Core;
using NSubstitute;
using Xunit;

namespace Lighthouse.Apps.Core.Tests
{
    public class BenchmarkTests
    {
        [Fact]
        public void NodeLatencyBenchmark_Run_NoTargets_Returns_Zero()
        {

            var testContainer = Substitute.For<ILighthouseServiceContainer>();
            var benchmark = new NodeLatencyBenchmark(testContainer, "127.0.0.1" );
            var result = benchmark.RunBenchmark();

            result.Should().Be(0);
        }
    }

    public class BenchmarkRunnerTests
    {
        [Fact]
        public void NodeLatencyBenchmark_Run_NoTargets_Returns_Zero()
        {

            var benchmark = new ZeroTimeBenchark();
            var result = benchmark.RunBenchmark();

            result.Should().Be(0);
        }
    }

    internal class ZeroTimeBenchark : IBenchmark
    {
        public void Run()
        {
            // do absolutely nothing
        }
    }

}
