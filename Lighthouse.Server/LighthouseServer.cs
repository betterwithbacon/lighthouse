using Lighthouse.Core;
using Lighthouse.Core.Deployment;
using System;
using System.Collections.Generic;

namespace Lighthouse.Server
{
    public class LighthouseServer
    {
		public LighthouseServer(Action<string> localLogger)
		{
			LogLocally = localLogger;
		}

		protected Action<string> LogLocally { get; set; }

		public void Start()
		{
			Log(LogLevel.Debug, "Lighthouse server starting");

			Initialize();

			StartThreads();
		}

		private void StartThreads()
		{

		}

		private void Initialize()
		{		

		}

		public void Launch(IEnumerable<LighthouseAppLaunchConfig> servicesToStart)
		{
			
		}

		protected void Log(LogLevel level, string message)
		{
			// ALL messages are logged locally for now
			LogLocally(message);


		}
	}
}