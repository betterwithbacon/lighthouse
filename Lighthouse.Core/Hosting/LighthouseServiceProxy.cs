using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Hosting
{
	/// <summary>
	/// A proxy for services not hosted within the context. They provide generic remoting of commands and responses
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class LighthouseServiceProxy<T>
		where T : class, ILighthouseService
	{
		public T Service { get; private set; }
		public T RemoteService { get; private set; }
		private readonly ILighthouseServiceContainerConnection SourceContainerConnection;

		public LighthouseServiceProxy(ILighthouseServiceContainerConnection sourceContainer)
			: this(sourceContainer,null)
		{

		}

		public LighthouseServiceProxy(ILighthouseServiceContainerConnection sourceContainer, T remoteService = default)
		{
			SourceContainerConnection = sourceContainer;

			//TODO: still workshopping whether this is needed or not, because we can't use ther direct object anyway.....or can we? if it's local
			RemoteService = remoteService;

			// build out a mock of thet original object using NSubstitute, and wire up the remoting
			// is this genius or madness?!
			Wireup();
		}

		private void Wireup()
		{
			// we don't need to init this service, because it's already initialized somewher eelse

			// if the service is local, then no need to "intercept" the service calls
			if (RemoteService != null)
			{
				// colpy the initial state of the service across?
				// this feels fundamentally wrong, it seems as if, the point of this architecture is that the services are purely methods
				Service = RemoteService;
			}
			else
			{
				// create a barebone service
				//Service = Activator.CreateInstance<T>();
				Service = Substitute.For<T>();
				Service.When((callThis) => { Console.WriteLine("called: " + Service); });
			}
		}	
	}
}
