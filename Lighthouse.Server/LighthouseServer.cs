using Lighthouse.Core;
using Lighthouse.Core.Deployment;
using Lighthouse.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lighthouse.Server
{
    public class LighthouseServer : ILighthouseServiceContext
    {
		private readonly List<ILighthouseService> Services;
		protected Action<string> LogLocally { get; set; }
		private readonly Dictionary<Guid, Task> ServiceTasks = new Dictionary<Guid, Task>();

		private bool IsRunning { get; set; }

		public LighthouseServer(Action<string> localLogger)
		{
			LogLocally = localLogger;
			Services = new List<ILighthouseService>();			
		}

		public void Start()
		{
			Log(LogLevel.Debug, "Lighthouse server starting");

			StartMonitor();

			IsRunning = true;
		}

		void AssertIsRunning()
		{
			if (!IsRunning)
				throw new InvalidOperationException("Lighthouse server is not running.");	
		}

		private void StartMonitor()
		{

		}

		public void Launch(IEnumerable<LighthouseAppLaunchConfig> launchConfigs)
		{
			foreach(var launchConfig in launchConfigs)
			{
				Launch(launchConfig);
			}
		}

		public void Launch(LighthouseAppLaunchConfig launchConfig)
		{
			AssertIsRunning();

			Log(LogLevel.Debug, $"Attempting to start app: {launchConfig.Name}");

			if (!(Activator.CreateInstance(launchConfig.Type) is ILighthouseService service))
				throw new ApplicationException($"App launch config doesn't represent Lighthouse app. {launchConfig.Type.AssemblyQualifiedName}");

			// put the service in a runnable state
			var serviceId = Guid.NewGuid();
			service.Initialize(this, serviceId);

			// start it, in a separate thread, that will run the business logic for this
			var startedTask = Task.Run(() => service.Start()); // fire and forget
			ServiceTasks.Add(serviceId, startedTask);

			Services.Add(service);
		}

		public void Log(LogLevel level, string message)
		{
			// ALL messages are logged locally for now
			LogLocally(message);


		}

		public IEnumerable<ILighthouseService> GetRunningServices()
			=> Services.Where(s => s.RunState > LighthouseServiceRunState.PendingStart && s.RunState < LighthouseServiceRunState.PendingStop);		
	}
}