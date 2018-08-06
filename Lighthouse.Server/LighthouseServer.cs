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

		List<ILighthouseServiceComponent> Components { get; set; }
		private ILighthouseServiceConfigurationContext ConfigurationContext { get; set; }
		protected Action<string> LogLocally { get; set; }

		internal void LoadApps(IEnumerable<LighthouseAppLaunchConfig> servicesToRun)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnLoadComponent(ILighthouseServiceComponent component)
		{ }

		public void Start()
		{
			Log(LogLevel.Debug, "Lighthouse server starting");

			Initialize();

			StartThreads();

			var _ = Console.ReadKey(); // block the main thread.
		}

		private void StartThreads()
		{

		}

		private void Initialize()
		{
			LoadConfiguration();

			Components.ForEach(LoadComponent);
		}

		protected virtual void LoadConfiguration()
		{
			ConfigurationContext = new StandardServiceConfig();
		}

		private void LoadComponent(ILighthouseServiceComponent component)
		{
			OnLoadComponent(component);
		}

		protected void Log(LogLevel level, string message)
		{
			// ALL messages are logged locally for now
			LogLocally(message);


		}
	}
}