using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace Lighthouse.Server.Management
{
	
	public class HttpLighthouseManagementServer :  LighthouseServiceBase //, IHttpManagementInterface
	{
		private HttpListener Listener { get; }
		private Thread ListeningThread; // TODO: I want ALL threads managed within the lighthouse, so instead of this thread being spawned here, I want the container to "own" it.
		private readonly ConcurrentBag<string> Routes = new ConcurrentBag<string>();
		public int Port { get; } 
		// the local IPs can be added as default routes to listen for other Lighthouse servers on the server
		public IList<string> DefaultIPs = new[] { LighthouseContainerCommunicationUtil.LOCAL_SERVER_ADDRESS, "localhost"};

		public HttpLighthouseManagementServer(
			int port = LighthouseContainerCommunicationUtil.DEFAULT_SERVER_PORT, 
			string[] ips = null, 
			bool registerDefaultRoutes = true)
		{
			Port = port;

			Listener = new HttpListener
			{
				AuthenticationSchemes = AuthenticationSchemes.Anonymous
			};

			if (ips != null)
			{
				foreach (var ip in ips.Where(NetworkUtil.IsIp).SelectMany((i) => GetListenerRoutesForIP(i, port)))
				{
					Listener.Prefixes.Add(ip);
				}
			}
			
			if(registerDefaultRoutes)
			{
				foreach(var route in DefaultIPs.SelectMany((i) => GetListenerRoutesForIP(i,port)))
				{
					Listener.Prefixes.Add(route);
				}
			}
		}

		public static IEnumerable<string> GetListenerRoutesForIP(string rawIp, int port)
		{


			// right now we only support HTTP for the listener, but if more 
			yield return $"http://{rawIp}:{port}/";
		}

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

		/// <summary>
		/// Blocks the current thread until a request is received. That work is then queued separately, and then it waits again.
		/// </summary>
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

			var route = CleanRoute(context.Request.Url.AbsolutePath);

			Container.Log(Core.Logging.LogLevel.Debug, Core.Logging.LogType.Info, this, $"Management server request received: Route: {route}. Payload: {payload}");

			string response = null, errorText = null;
			
			try
			{
				response = Route(route, payload);
			}
			catch(Exception e)
			{
				errorText = e.Message;
			}
			
			using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
			{
				// TODO: will probably need better handling around making the responses more formalized.
				if(!string.IsNullOrEmpty(response))
				{
					context.Response.StatusCode = (int)HttpStatusCode.OK;
					context.Response.StatusDescription = LighthouseContainerCommunicationUtil.Messages.OK;
					writer.Write(response);
				}
				else if (errorText != null)
				{
					context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
					context.Response.StatusDescription = LighthouseContainerCommunicationUtil.Messages.ERROR;
					writer.Write(errorText);
				}
				else
				{
					// something has happened, that hasn't thrown an exception, but also provided no message
					// this should be pretty uncommmon
					context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
					context.Response.StatusDescription = LighthouseContainerCommunicationUtil.Messages.UNSUPPORTED;					
				}
			}

			context.Response.Close();
		}

		public string Route(string routeName, string payload)
		{
            // delegate each question to the container itself
            // IDK, I don't want to rebuild ASP MVC controllers, i just want very terse mapping
            // my concern is, lets say we add 5 endpopints, and there's 3 management interfaces, I don't want them to have to do the mappinb, as well, just to sort of proxy it. 
            // I think the management interfaces are purely abstractions for the 
            //if(Enum.TryParse<ManagementRequestType>(routeName,true, out var requestType))
            //{
            //             //var managementResponse = Container.HandleManagementRequest(requestType, payload);
            //             //return managementResponse.Message;
            //             return null;
            //}
            //else
            //{
            //	return null;
            //}
            return null;
		}

		public static string CleanRoute(string input)
		{
			// just replace the slashes
			return input?.Replace("/", "");
		}
	}
}
