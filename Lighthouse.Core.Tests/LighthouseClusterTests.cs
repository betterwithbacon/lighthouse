

using System;
using Lighthouse.Server;
using Lighthouse.Core.Utils;
using Lighthouse.Core.Hosting;

namespace Lighthouse.Core.Tests
{
    public class LighthouseClusterTests
    {
        public void Lighthouse_ClusterForms()
        {
            var vLan = new VirtualNetwork();

            var master = new LighthouseServer();
            
            var slave1 = new LighthouseServer();
            var slave2 = new LighthouseServer();

            //Uri.TryCreate()
            
            // master.Connect("http://169.0.0.1".ToUri()); // master
            master.Connect("http://169.0.0.2".ToUri()); // slave1
            master.Connect("http://169.0.0.3".ToUri()); // slave2


        }
    }
}