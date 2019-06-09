using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.Deployment
{
	//public class ServiceInstallationHandler : IAppCommandHandler
	//{
	//	// TODO: convert this to an attribute
	//	public const string COMMAND_NAME = "install";

	//	public static class Arguments
	//	{
	//		public static string APP_NAME = "appName";
	//		public static string TARGET_SERVER = "targetServer";
	//	}

	//	public async Task Handle(IDictionary<string, string> argValues, IAppContext context)
	//	{
	//		if (!argValues.TryGetValue(Arguments.APP_NAME, out var appNameToInstall))
	//		{
	//			context.InvalidArgument(Arguments.APP_NAME, CliApp.CommandPrompts.MISSING_ARGUMENT);
	//			return;
	//		}

	//		// literally create a server to do the install.
	//		var server = context.GetResource<ILighthouseServiceContainer>();

	//		if (server == null)
	//			throw new Exception("No lighthouse server could be found");

	//		// The installation target might be somewhere else, but this is the proper place to get it started
	//		//var server = new LighthouseServer("lighthouse-cli", commandCall.App.Log, Environment.CurrentDirectory);

	//		// global config
	//		//commandCall.App.ContainerOnBuild?.Invoke(server);

	//		server.Start();
	//		var foundServices = server.FindServiceDescriptor(appNameToInstall).ToList();

	//		if (foundServices.Count == 0)
	//		{
	//			context.Fault($"No services found with name: {appNameToInstall}");
	//			return;
	//		}

	//		if (foundServices.Count > 1)
	//		{
	//			context.Fault($"Multiple services found with name: {appNameToInstall}");
	//			return;
	//		}

	//		var serviceToInstall = foundServices.Single();

	//		// there's only one service, so register that service with a server
	//		// installations are PERMANENT associations, so the server needs to own that process
	//		// which means using this temporary server to do it, makes little sense

	//		ILighthouseServiceContainerConnection lighthouseServerConnection = null;

	//		if (!argValues.TryGetValue(Arguments.TARGET_SERVER, out var lighthouseServerToTarget))
	//		{
	//			// look for a local server on this machine
	//			var otherServers = server.FindServers().ToList();

	//			// if one can't be found, ask for a target machine
	//			if (otherServers.Count == 0)
	//			{
	//				context.Fault($"No target Lighthouse servers found.");
	//				return;
	//			}

	//			// if there are MULTIPLE local servers on this machine, then ask for a target machine
	//			if (otherServers.Count > 1)
	//			{
	//				context.Fault($"Multiple Lighthouse servers found. Specify the URI of the target lighthouse server.");
	//				return;
	//			}

	//			lighthouseServerConnection = otherServers.Single();
	//		}
	//		else
	//		{
	//			if (!Uri.TryCreate(lighthouseServerToTarget, UriKind.Absolute, out var lighthouseServerUri))
	//			{
	//				context.InvalidArgument(Arguments.TARGET_SERVER, $"Invalid URI:{lighthouseServerToTarget}");
	//				return;
	//			}

	//			lighthouseServerConnection = server.Connect(lighthouseServerUri);

	//			context.Log($"Connection made to {lighthouseServerUri}");
	//		}

	//		if(lighthouseServerConnection == null)
	//		{
	//			context.Fault($"A lighthouse connection couldn't be made.");
	//			return;
	//		}

	//		var response = await lighthouseServerConnection.SubmitManagementRequest(
	//			ServerManagementRequestType.Install,
	//			new Dictionary<string, object> {
	//				{
	//					ServerManagementRequest.RequestTypes.Install.Arguments.ServiceName,
	//					appNameToInstall
	//				}}
	//			);
			
	//		if (response == null)
	//		{
	//			context.Fault("No response was received from lighthouse.");
	//			return;
	//		}
	//		else if (response.WasSuccessful)
	//		{
	//			context.Finish("Service installed.");
	//			return;
	//		}
	//		else
	//		{
	//			context.Fault($"Installation failed: {response.Message}");
	//			return;
	//		}
	//	}
	//}
}
