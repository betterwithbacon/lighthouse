using Lighthouse.Core;
using Lighthouse.Core.Hosting;
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
			Listener = new HttpListener();
			//Routes.Add("");
			//foreach (var uri in uris)
			//	Listener.Prefixes.Add(uri);

			Listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
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
			Thread.Sleep(1000);
			var requestPayload = new StreamReader(
				context.Request.InputStream,
				context.Request.ContentEncoding
			).ReadToEnd();

			var cleaned_data = System.Web.HttpUtility.UrlDecode(requestPayload);

			context.Response.StatusCode = 200;
			context.Response.StatusDescription = LighthouseContainerCommunicationUtil.Messages.OK;

			var route = context.Request.Url.AbsolutePath;

			//RouteResponses.TryGetValue(route, out var responseFunc);
			
			using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
			{
				if (responseFunc == null)
					writer.WriteLine("INVALID ROUTE: NO RESPONSE");
				else
					writer.WriteLine(responseFunc(cleaned_data));
			}

			context.Response.Close();
		}

		public void 
	}
}
