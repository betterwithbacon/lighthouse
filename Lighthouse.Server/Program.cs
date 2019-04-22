using Lighthouse.Core.UI;
using System;

namespace Lighthouse.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting lighthouse server");
			
			//if (args.Length > 1)
			//{
			//	RunConsoleMode(args);
			//}
			//else
			//{
				RunHeadlessMode();
			//} 
		}

		static void RunConsoleMode(string[] args)
		{
			// setup the app
			var app = new CliApp("lighthouse", Console.WriteLine, Console.ReadLine, Console.ReadKey);

			app.AddCommand("test")
					.AddArgument("appConfigJsonFile").Command.App
				.AddCommand("run")
					.AddArgument("appConfigJsonFile").Command.App
				.AddCommand("install")
					.AddArgument("appName").Command.App
				.AddCommand("deploy")
					.AddArgument("appConfigJsonFile")
					.AddArgument("environment") //, "local", "< server IP >")
						.AddHint("local", "Deploy to this machine.")
						.AddHint("serverIP", "Deploy to the target IP. This machine should have a Lighthouse Monitor on it.");

			System.Diagnostics.Debugger.Launch();

			app.Start(args);
		}

		static void RunHeadlessMode()
		{
			var server = new LighthouseServer(localLogger: (message) => Console.WriteLine(message));			
			server.AddAvailableNetworkProviders();
			server.AddAvailableFileSystemProviders();

			server.Start();
			var _ = Console.ReadKey();
		}
    }
}
