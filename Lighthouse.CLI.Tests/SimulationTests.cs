using System;
using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Server;
using Xunit;

namespace Lighthouse.CLI.Tests
{
    public class SimulationTests
    {
        [Fact]
        public void LargeSimulation()
        {
            // all simulatiln traffic will flow through this
            var network = new VirtualNetwork();

            // spin up 3 nodes (to make it spicey)
            var container1 = new LighthouseServer();
            container1.RegisterResource(network);

            var container2 = new LighthouseServer();
            container2.RegisterResource(network);

            var container3 = new LighthouseServer();
            container3.RegisterResource(network);

        }
    }
}
