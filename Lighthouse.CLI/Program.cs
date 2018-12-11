using Lighthouse.CLI.Handlers.Deployments;
using Lighthouse.Core;
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
			var lighthouseCliApp = new LighthouseCliApp(Console.WriteLine, Console.ReadLine, Console.ReadKey);

			System.Diagnostics.Debugger.Launch();

			lighthouseCliApp.Start(args);
		}
	}

	public class LighthouseCliApp : CliApp
	{
		public LighthouseCliApp(Action<string> writeLine, Func<string> readLine, Func<ConsoleKeyInfo> readKey, 
			Func<bool, string, bool> onQuit = null, Action<ILighthouseServiceContainer> onServerBuild = null)
			: base("lighthouse-cli", writeLine,readLine, readKey)
		{
			this.AddResource(new LighthouseServer("lighthouse-cli", writeLine, Environment.CurrentDirectory));
			this.AddCommand("test")
				.AddArgument("appConfigJsonFile");
			this.AddCommand("run")
				.AddArgument("appConfigJsonFile");
			this.AddCommand<ServiceInstallationHandler>("install")
				.AddArgument(ServiceInstallationHandler.Arguments.APP_NAME);
			this.AddCommand("deploy")
				.AddArgument("appConfigJsonFile")
				.AddArgument("environment")
					.AddHint("local", "Deploy to this machine.")
					.AddHint("serverIP", "Deploy to the target IP. This machine should have a Lighthouse Monitor on it.");
		}
	}
}
