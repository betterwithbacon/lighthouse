using System;

namespace Lighthouse.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting lighthouse server");
			//var server = LighthouseLauncher<LighthouseServer>
			//	.Create<LighthouseServer>("Lighthouse Server")
			//	.AddLocalLogger((message) => Console.WriteLine(message))
			//	.Build();

			var server = new LighthouseServer((message) => Console.WriteLine(message));
			//server.AddLocalLogger((message) => Console.WriteLine(message));
				
			// start the server
			// it will load up it's local config, and connect to the repository.
			server.Start();

			// block the console thread
			var _ = Console.ReadKey();
		}
    }
}
