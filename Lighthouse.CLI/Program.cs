using Lighthouse.Core.UI;
using System;
using System.Collections.Generic;

namespace Lighthouse.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
			// setup the app
			var app = new CliApp("lighthouse", Console.WriteLine, Console.ReadLine, Console.ReadKey);

			app.AddCommand("test")
					.AddArgument("appConfigJsonFile").Command.App
				.AddCommand("run")
					.AddArgument("appConfigJsonFile").Command.App
				.AddCommand("deploy")
					.AddArgument("appConfigJsonFile")
					.AddArgument("environment") //, "local", "< server IP >")
						.AddHint("local", "Deploy to this machine.")
						.AddHint("serverIP", "Deploy to the target IP. This machine should have a Lighthouse Monitor on it.");

			System.Diagnostics.Debugger.Launch();
			//app.Start(new[] { "test=yup" });
			app.Start(args);
		}
	}
}
