using BusDriver.Core.Events;
using BusDriver.Core.Scheduling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WarehouseCore;

namespace Lighthouse.Core.Apps.Warehouse
{
    public class WarehouseServer : LighthouseServiceBase, IWarehouse<string,string>
    {
		private readonly WarehouseCore.Warehouse warehouse = new WarehouseCore.Warehouse(initImmediately:false);
		private readonly ConcurrentBag<WarehouseServer> RemoteWarehouseServers = new ConcurrentBag<WarehouseServer>();
		private readonly EventContext busDriver = new EventContext();
		
		protected override void OnStart()
		{
			// start a local warehouse
			warehouse.Initialize();
		}

		protected override void OnAfterStart()
		{
			// schedule some work to be done
			busDriver.AddSchedule(new Schedule { }, (time) => { PerformStorageMaintenance(time); });

			// populate the remote warehouses			
			LoadRemoteWarehouses();
		}

		private void LoadRemoteWarehouses()
		{
			// the Lighthouse context should know about the other services that are running
			foreach (var remoteWarehouseServer in Context.FindServices<WarehouseServer>())
				RemoteWarehouseServers.Add(remoteWarehouseServer);

			// this is where an network discovery will occur. to reach other points, not local to this lighthouse runtime.
			// currently, this isn't implemented, but ideally
			foreach (var remoteWarehouseServer in Context.FindRemoteServices<WarehouseServer>())
				RemoteWarehouseServers.Add(remoteWarehouseServer);			
		}

		public IEnumerable<IShelf> ResolveShelves(IEnumerable<LoadingDockPolicy> policies)			
		{
			return AllWarehouses.SelectMany(war => war.ResolveShelves(policies));
		}

		public IEnumerable<IWarehouse<string,string>> AllWarehouses => RemoteWarehouseServers.SelectMany(ws => ws.AllWarehouses).Concat(new[] { warehouse });
		
		private void PerformStorageMaintenance(DateTime date)
		{
		}

		void IWarehouse<string,string>.Initialize()
		{
			throw new NotImplementedException("This warehouse server should be initialized within a lighthouse context.");
		}

		public Receipt Store(string key, IStorageScope scope, IList<string> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			// when items are stored, store them in the local warehouse. Policy syncing will happen somewhere else
			var receipt = warehouse.Store(key, scope, data, loadingDockPolicies);			
			RaiseStatusUpdated("Data stored: ");

			return receipt;
		}

		public void Append(string key, IStorageScope scope, IEnumerable<string> data, IEnumerable<LoadingDockPolicy> loadingDockPolicies)
		{
			// when items are stored, store them in the local warehouse. Policy syncing will happen somewhere else
			warehouse.Append(key, scope, data, loadingDockPolicies);
			
		}

		public IEnumerable<string> Retrieve(string key, IStorageScope scope)
		{
			return warehouse.Retrieve(key, scope);
		}
	}
}
