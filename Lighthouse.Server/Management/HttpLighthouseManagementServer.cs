using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Management;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Lighthouse.Server.Management
{
	
	public class HttpLighthouseManagementServer :  LighthouseServiceBase, ILighthouseManagementInterface
	{
		public HttpListener Listener { get; }
		public 
		//private readonly ConcurrentDictionary<string, Func<string, string>> RouteResponses = new ConcurrentDictionary<string, Func<string, string>>();
		Thread ListeningThread; // TODO: I want ALL threads managed within the lighthouse, so instead of this thread being spawned here, I want the container to "own" it.
		private readonly ConcurrentBag<string> Routes = new ConcurrentBag<string>();

		public HttpLighthouseManagementServer()
		{
			Listener = new HttpListener
			{
				AuthenticationSchemes = AuthenticationSchemes.Anonymous
			};

			//Routes.Add("");
			//foreach (var uri in uris)
			//	Listener.Prefixes.Add(uri);
		}

		//public void AddRoute(string route, Func<string, string> handler)
		//{
		//	RouteResponses.TryAdd(route, handler);
		//}

		protected override void OnStart()
		{
			base.OnStart();

			Listener.Start();
			ListeningThread = new Thread(new ParameterizedThreadStart(WaitForRequests));
			ListeningThread.Start();
		}

		protected override void OnStop()
		{
			base.OnStop();
			ListeningThread.Abort();
		}

		private void WaitForRequests(object _)
		{
			while (true)
			{
				ProcessRequest();
			}
		}

		private void ProcessRequest()
		{
			var result = Listener.BeginGetContext(ListenerCallback, Listener);
			result.AsyncWaitHandle.WaitOne();
		}

		private void ListenerCallback(IAsyncResult result)
		{
			var context = Listener.EndGetContext(result);
			
			var requestPayload = new StreamReader(
				context.Request.InputStream,
				context.Request.ContentEncoding
			).ReadToEnd();

			var payload = System.Web.HttpUtility.UrlDecode(requestPayload);

			var route = context.Request.Url.AbsolutePath;

			Container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, this, $"Management server request received: Route: {route}. Payload: {payload}");

			var response = Route(route, payload);
			
			//RouteResponses.TryGetValue(route, out var responseFunc);

			using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
			{
				// TODO: will probably need better handling around making the responses more formalized.
				if(!string.IsNullOrEmpty(response))
				{
					context.Response.StatusCode = (int)HttpStatusCode.OK;
					context.Response.StatusDescription = LighthouseContainerCommunicationUtil.Messages.OK;
					writer.Write(response);
				}
				else
				{
					context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
					context.Response.StatusDescription = LighthouseContainerCommunicationUtil.Messages.OK;
				}
			}

			context.Response.Close();
		}

		public string Route(string routeName, string payload)
		{
			// delegate each question to the container itself
			// IDK, I don't want to rebuild ASP MVC controllers, ijust want very terse mapping
			// my concern is, lets say we add 5 endpopints, and there's 3 management interfaces, I don't want them to have to do the mappinb, as well, just to sort of proxy it. 
			// I think the management interfaces are purely abstractions for the 

			if(Enum.TryParse<ManagementRequestType>(routeName, out var requestType))
			{
				var managementResponse = Container.SubmitManagementRequest(requestType, payload);
				return managementResponse.Message;
			}
			else
			{
				return null;
			}
		}
	}
}
