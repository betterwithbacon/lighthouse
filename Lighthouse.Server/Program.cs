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
			
			// start the server, it will wait before services are injected
			server.Start();

			var servicesToRun = LighthouseLauncher
				.FindServices( new LighthouseAppLocation[]
					{
						new LighthouseFileSystemLocation { Directory = $"{Environment.CurrentDirectory}\\Apps" },
						new LighthouseTypeBasedLocation { AssemblyPath = $"{Environment.CurrentDirectory}\\Lighthouse.Core.App.dll" }
					}
				);

			// load the apps
			server.LoadApps(servicesToRun);
		}
    }
}
