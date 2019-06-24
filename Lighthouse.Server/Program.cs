using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.Services;
using System;
using System.Threading.Tasks;

namespace Lighthouse.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting lighthouse server");

            var server = new LighthouseServer(
                localLogger: Console.WriteLine);

            server.Start();
            server.Launch(typeof(PingService));
            _ = Console.ReadLine();
            await server.Stop();
        }
    }
}
