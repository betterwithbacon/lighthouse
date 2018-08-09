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
		private readonly Dictionary<string, Task> ServiceTasks = new Dictionary<string, Task>();
		private readonly CancellationTokenSource CancellationTokenSource;

		public event StatusUpdatedEventHandler StatusUpdated;

		private bool IsRunning { get; set; }

		public LighthouseServer(Action<string> localLogger)
		{
			LogLocally = localLogger;
			Services = new List<ILighthouseService>();
			CancellationTokenSource = new CancellationTokenSource();
		}

		public void Start()
		{
			Log(LogLevel.Debug,this, "Lighthouse server starting");

			StartMonitor();
			
			IsRunning = true;
		}

		public void Stop()
		{
			// call stop on all of the services
			Services.ForEach(service => { service.Stop(); });

			int iter = 1;
			
			// just do some kindness before killing the thread. Most thread terminations should be instantaneous.
			while(GetRunningServices().Any() && iter < 3)
			{
				Log(LogLevel.Debug, this, $"[Stopping] Waiting for services to finish. attempt {iter}");
				Thread.Sleep(500);
				iter++;
			}

			// wait some period of time, for the services to stop by themselves, then just kill the threads out right.
			CancellationTokenSource.Cancel();

			Log(LogLevel.Debug,this, "[Lighthouse Server Stopped]");
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

			Log(LogLevel.Debug, this, $"Attempting to start app: {launchConfig.Name} (ID: {launchConfig.Id}).");

			if (!(Activator.CreateInstance(launchConfig.Type) is ILighthouseService service))
				throw new ApplicationException($"App launch config doesn't represent Lighthouse app. {launchConfig.Type.AssemblyQualifiedName}");

			// put the service in a runnable state
			var serviceId = GetNewPseudoRandomString();
			service.StatusUpdated += Service_StatusUpdated;
			service.Initialize(this, serviceId);
			
			// start it, in a separate thread, that will run the business logic for this
			var startedTask = Task.Run(() => service.Start(), CancellationTokenSource.Token); // fire and forget
			ServiceTasks.Add(serviceId, startedTask);

			Services.Add(service);
		}

		private void Service_StatusUpdated(ILighthouseComponent owner, string status)
		{
			Log(LogLevel.Debug, owner, status);
		}

		void RaiseStatusUpdated(string statusChangeMessage)
		{
			StatusUpdated?.Invoke(this, statusChangeMessage);
		}

		public void Log(LogLevel level, ILighthouseComponent sender, string message)
		{
			string log = $"[{DateTime.Now.ToString("HH:mm:fff")}] [{sender}]: {message}";

			// ALL messages are logged locally for now
			LogLocally(log);
		}

		public IEnumerable<ILighthouseService> GetRunningServices()
			=> Services.Where(s => s.RunState > LighthouseServiceRunState.PendingStart && s.RunState < LighthouseServiceRunState.PendingStop);

		static string GetNewPseudoRandomString()
		{
			var rawId = Guid.NewGuid().ToString();
			return rawId.Substring(rawId.LastIndexOf('-') + 1);
		}

		public override string ToString()
		{
			return "Lighthouse Server";
		}
	}
}