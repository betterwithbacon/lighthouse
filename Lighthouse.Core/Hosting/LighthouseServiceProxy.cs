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
		where T : ILighthouseService
	{
		public T Service { get; }

		public LighthouseServiceProxy()
		{
			// build out a mock of thet original object using NSubstitute, and wire up the remoting
			// is this genius or madness?!
			Wireup();
		}

		private void Wireup()
		{
			
		}
	}
}
