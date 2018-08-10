using BusDriver.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseCore;

namespace Lighthouse.Core.Apps.Warehouse
{
    public class WarehouserServer : LighthouseServiceBase
    {
		private readonly WarehouseCore.Warehouse warehouse = new WarehouseCore.Warehouse(initImmediately:false);
		private readonly EventContext busDriver = new EventContext();

		protected override void OnStart()
		{
			// start your own local warehouse
			warehouse.Initialize();
			BusDriver.Core.Events.Bus
			
		}

		public IEnumerable<IShelf> GetShelves()
		{
			return null;
		}
	}
}
