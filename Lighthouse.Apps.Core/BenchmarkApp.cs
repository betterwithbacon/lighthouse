using System;
using System.Threading.Tasks;
using Lighthouse.Core;
using Lighthouse.Core.Configuration.ServiceDiscovery;

namespace Lighthouse.Apps.Core
{
    public enum BenchmarkType
    {
        Latency,
        OPS
    }

    public enum BenchmarkTarget
    {
        Node,
        
    }

    [ExternalLighthouseService("benchamrk")]
    public class BenchmarkApp : LighthouseServiceBase
    {
        public BenchmarkType Type { get; set; }

        public BenchmarkTarget Target { get; set; }

        protected override Task OnStart()
        {
            // what is being benchmarked


        }
    }

    public class NodeLatencyBenchmark : IBenchmark
    {

    }

    public interface IBenchmark
    {

    }
}
