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
			server.Start();


			// block the console thread
			var _ = Console.ReadKey();

			var servicesToRun = LighthouseLauncher
				.FindServices( new ILighthouseAppLocation[]
					{
						new LighthouseFileSystemLocation { Directory = $"{Environment.CurrentDirectory}\\Apps" } //,
						//new LighthouseTypeBasedLocation { AssemblyPath = $"{Environment.CurrentDirectory}\\Lighthouse.Core.App.dll" }
					}, (o,i) => Console.WriteLine($"{o}:{i}")
				);

			// load the apps
			server.Launch(servicesToRun);
		}
    }
}
