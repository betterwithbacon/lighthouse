using Lighthouse.Core.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core
{
	public class LighthouseServiceRun
	{
		public readonly ILighthouseService Service;
		public readonly string ID;
		public readonly IProducerConsumerCollection<Exception> Exceptions = new ConcurrentBag<Exception>();
		internal readonly Task Task;

		public int TaskId
		{
			get
			{
				return Task?.Id ?? -1;
			}
		}

		public LighthouseServiceRun(ILighthouseService service, Task task)
		{
			Service = service;
			Task = task;
			ID = LighthouseComponentLifetime.GenerateSessionIdentifier(service);
		}
	}
}
