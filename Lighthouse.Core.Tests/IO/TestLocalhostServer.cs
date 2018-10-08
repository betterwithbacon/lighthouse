using Lighthouse.Core.Hosting;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Lighthouse.Core.Tests.IO
{
	public class TestLocalhostServer
	{
		public HttpListener Listener { get; }
		private readonly ConcurrentDictionary<string, Func<string, string>> RouteResponses = new ConcurrentDictionary<string, Func<string, string>>();
		
		public TestLocalhostServer(params string[] uris)
		{
			Listener = new HttpListener();
			foreach(var uri in uris)
				Listener.Prefixes.Add(uri);
			Listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
		}

		public void  AddRouteResponse(string route, Func<string,string> handler)
		{
			RouteResponses.TryAdd(route, handler);
		}

		Thread ListeningThread;
		public void Start()
		{
			Listener.Start();
			ListeningThread = new Thread(new ParameterizedThreadStart(WaitForRequests));
			ListeningThread.Start();			
		}

		public void Stop()
		{
			ListeningThread.Abort();
		}

		private async Task Listen()
		{
			await Listener.GetContextAsync();
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

			RouteResponses.TryGetValue(route, out var responseFunc);

			using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
			{
				if (responseFunc == null)
					writer.WriteLine("INVALID ROUTE: NO RESPONSE");
				else
					writer.WriteLine(responseFunc(cleaned_data));				
			}

			context.Response.Close();
		}
	}
}
