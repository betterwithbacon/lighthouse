using System;
using System.Collections.Generic;

namespace Lighthouse.Core
{
    public abstract class LighthouseServiceBase : ILighthouseService
    {
		protected ILighthouseServiceContext Context { get; private set; }

		public string Id { get; private set; }

		public LighthouseServiceRunState RunState { get; protected set; }

		public event StatusUpdatedEventHandler StatusUpdated;

		public virtual void Start()
        {			
			OnStart();
			RaiseStatusUpdated(LighthouseServiceRunState.Running);
			OnAfterStart();
		}

		protected virtual void OnStart()
		{
		}
		
		protected virtual void OnAfterStart()
		{
		}

		public virtual void Stop()
		{
			RaiseStatusUpdated(LighthouseServiceRunState.PendingStop);
			StatusUpdated?.Invoke(this, $"Service shutting down.");
			OnStop();
			RaiseStatusUpdated(LighthouseServiceRunState.Stopped);
		}

		protected virtual void OnStop()
		{
		}

		protected void RaiseStatusUpdated(LighthouseServiceRunState newState)
		{
			StatusUpdated?.Invoke(this, $"Status changing from {RunState} to {newState}");
			RunState = newState;
		}

		public void Initialize(ILighthouseServiceContext context, string id)
		{
			Context = context;
			Id = id;
			RaiseStatusUpdated(LighthouseServiceRunState.PendingStart);
		}

		public override string ToString()
		{
			return $"[{GetType().Name}|{Id}]";
		}
	}
}


//public Guid Id { get; private set; }

//public LighthouseServiceRunState RunState { get; set; }

//public event StatusUpdatedEventHandler StatusUpdated;

//private ILighthouseServiceContext Context { get; set; }

//public void Initialize(ILighthouseServiceContext context, Guid id)
//{
//	RunState = LighthouseServiceRunState.PendingStart;
//	Context = context;
//	Id = id;
//}

//public void Start()
//{
//	RunState = LighthouseServiceRunState.Running;
//	System.Timers.Timer timer = new System.Timers.Timer(10);
//	timer.Elapsed += (o, e) => Context.Log(Core.Logging.LogLevel.Info, "event" + DateTime.Now.ToString("ss:fff"));
//}

//public void Stop()
//{
//	throw new NotImplementedException();
//}
