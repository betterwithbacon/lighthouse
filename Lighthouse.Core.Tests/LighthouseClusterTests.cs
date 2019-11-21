

using System;
using Lighthouse.Server;
using Lighthouse.Core.Utils;
using Lighthouse.Core.Hosting;
using Lighthouse.Apps.Core;
using System.Threading.Tasks;

namespace Lighthouse.Core.Tests
{
    public class LighthouseClusterTests
    {
        public async Task Lighthouse_ClusterForms()
        {
            var vLan = new VirtualNetwork();

            var master = new LighthouseServer("master");
            var masterUri = "http://169.0.0.1".ToUri();
            vLan.Register(master, masterUri);
            master.RegisterResourceProvider(vLan);



            var slave1 = new LighthouseServer("slave1");
            var slave1Uri = "http://169.0.0.2".ToUri();
            vLan.Register(slave1, slave1Uri);
            slave1.RegisterResourceProvider(vLan);


            var slave2 = new LighthouseServer("slave2");
            var slave2Uri = "http://169.0.0.3".ToUri();
            vLan.Register(slave2, slave2Uri);
            slave2.RegisterResourceProvider(vLan);

            var benchmark = new BenchmarkApp
            {
                Target = BenchmarkTarget.Node,
                Type = BenchmarkType.Latency
            };
            benchmark.Args["isTargetName"] = "slave1";

            
            // have the master "discover" the other 2 servers on the network and ping them both
            await master.Launch(benchmark);

            
                                 
        }
    }
}