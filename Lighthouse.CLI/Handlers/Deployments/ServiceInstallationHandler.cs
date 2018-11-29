using Lighthouse.Core.Hosting;
using Lighthouse.Core.Management;
using Lighthouse.Core.UI;
using Lighthouse.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.CLI.Handlers.Deployments
{
	public class ServiceInstallationHandler : IAppCommandExecutor
	{
		public static class Arguments
		{
			public static string APP_NAME = "appName";
			public static string TARGET_SERVER = "targetServer";
		}

		public async Task Execute(AppCommandExecution commandCall)
		{
			var appNameToInstall = commandCall.FirstOrDefaultCommandArgValue(Arguments.APP_NAME);

			if (appNameToInstall == null)
				commandCall.App.InvalidArgument(Arguments.APP_NAME, CliApp.CommandPrompts.MISSING_ARGUMENT);

			// literally create a server to do the install. 
			// The installation target might be somewhere else, but this is the proper place to get it started
			var server = new LighthouseServer("lighthouse-cli", commandCall.App.Log, Environment.CurrentDirectory);
			server.Start();
			var foundServices = server.FindServiceDescriptor(appNameToInstall).ToList();

			if (foundServices.Count == 0)
				commandCall.App.Fault($"No services found with name: {appNameToInstall}");

			if (foundServices.Count > 1)
				commandCall.App.Fault($"Multiple services found with name: {appNameToInstall}");

			var serviceToInstall = foundServices.Single();

			// there's only one service, so register that service with a server
			// installations are PERMANENT associations, so the server needs to own that process
			// which means using this temporary server to do it, makes little sense

			var lighthouseServerToTarget = commandCall.FirstOrDefaultCommandArgValue(Arguments.APP_NAME);

			ILighthouseServiceContainerConnection lighthouseServerConnection = null;

			if (lighthouseServerToTarget == null)
			{
				// look for a local server on this machine
				var otherServers = server.FindServers().ToList();

				// if one can't be found, ask for a target machine
				if (otherServers.Count == 0)
				{
					commandCall.App.Fault($"No target Lighthouse servers found.");
				}

				// if there are MULTIPLE local servers on this machine, then ask for a target machine
				if (otherServers.Count > 1)
				{
					commandCall.App.Fault($"Multiple Lighthouse servers found. Specify the URI of the target lighthouse server.");
				}

				lighthouseServerConnection = otherServers.Single();
			}
			else
			{
				if (!Uri.TryCreate(lighthouseServerToTarget, UriKind.Absolute, out var lighthouseServerUri))
					commandCall.App.InvalidArgument(Arguments.APP_NAME, $"Invalid URI:{lighthouseServerToTarget}");

				lighthouseServerConnection = new NetworkLighthouseServiceContainerConnection(server, IPAddress.Parse(lighthouseServerUri.Host), lighthouseServerUri.Port);
			}

			if(lighthouseServerConnection == null)
			{
				commandCall.App.Fault($"A lighthjouse connection couldn't be made.");
			}

			var response = await lighthouseServerConnection.SubmitManagementRequest(new ServiceInstallationRequest(serviceToInstall));

			if (response.WasSuccessful)
				commandCall.App.Finish("Service installed.");
			else
				commandCall.App.Fault($"Installation failed: {response.Message}");
		}
	}
}
