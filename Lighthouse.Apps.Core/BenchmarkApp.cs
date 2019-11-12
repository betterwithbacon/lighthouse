using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public IDictionary<string, string> Args {get;set;}

        private IBenchmark Benchmark { get; set; }


        protected override async Task OnStart()
        {
            Benchmark = CreateBenchmark(Type, Target);
            await Task.Run(() => {
                    Benchmark.RunBenchmark();
                });
        }

        private IBenchmark CreateBenchmark(BenchmarkType benchmarkType, BenchmarkTarget target)
        {
            if(benchmarkType == BenchmarkType.Latency)
            {
                if (target == BenchmarkTarget.Node)
                {
                    return new NodeLatencyBenchmark(this.Container, Args["targetIp"]);
                }
            }

            return null;
        }
    }

    public class NodeLatencyBenchmark : IBenchmark
    {
        private Uri TargetUri { get; }
        private ILighthouseServiceContainer Container { get; }

        public NodeLatencyBenchmark(ILighthouseServiceContainer container, string targetIp)
        {
            this.Container = container;

            if(!Uri.TryCreate(targetIp, UriKind.Absolute, out var parsedUri))
            {
                TargetUri = parsedUri;
            }
            else
            {
                throw new Exception("Invalid target IP address.");
            }
        }

        public void Run()
        {
            // test latency, by sendign a request o the remote target, and validating the response

            // connect based on just name and not IP
            var connection = Container.Connect(TargetUri);

            if (!connection.TryConnect().GetAwaiter().GetResult())
            {
                throw new Exception("Connection failed to be created.");
            }
        }
    }

    public interface IBenchmark
    {
        void Run();
    }

    public static class BenchmarkRunner
    {
        /// <summary>
        /// Returns the total runtime in milliseconds that the benchmark took to run
        /// </summary>
        /// <param name="benchmark"></param>
        /// <returns></returns>
        public static long RunBenchmark(this IBenchmark benchmark)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            benchmark.Run();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
