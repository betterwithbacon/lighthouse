using Lighthouse.Core.Deployment;
using System;

namespace Lighthouse.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting lighthouse server");
			
			var server = new LighthouseServer(
				localLogger: (message) => Console.WriteLine(message)
			);

			// start the server
			// it will load up it's local config, and connect to the repository.
			server.Start();

			// block the console thread
			var _ = Console.ReadKey();
			
			// find the services on the local file system
			//var servicesToRun = LighthouseLauncher
			//	.FindServices( new ILighthouseAppLocation[]
			//		{
			//			new LighthouseFileSystemLocation(server) { Directory = $"{Environment.CurrentDirectory}\\Apps" } //,
			//			//new LighthouseTypeBasedLocation { AssemblyPath = $"{Environment.CurrentDirectory}\\Lighthouse.Core.App.dll" }
			//		}, (o,i) => Console.WriteLine($"{o}:{i}")
			//	);

			// load the apps
			//server.Launch(servicesToRun);
		}
    }
}
