using Lighthouse.CLI.Handlers.Deployments;
using Lighthouse.Core.UI;
using Lighthouse.Server;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lighthouse.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
			// setup the app
			var app = new CliApp("lighthouse", Console.WriteLine, Console.ReadLine, Console.ReadKey);

			app.AddCommand("test")
				.AddArgument("appConfigJsonFile");
			app.AddCommand("run")
				.AddArgument("appConfigJsonFile");
			app.AddCommand<ServiceInstallationHandler>("install")
				.AddArgument(ServiceInstallationHandler.Arguments.APP_NAME);
			app.AddCommand("deploy")
				.AddArgument("appConfigJsonFile")
				.AddArgument("environment")	
					.AddHint("local", "Deploy to this machine.")
					.AddHint("serverIP", "Deploy to the target IP. This machine should have a Lighthouse Monitor on it.");

			System.Diagnostics.Debugger.Launch();
			
			app.Start(args);
		}
	}
}
