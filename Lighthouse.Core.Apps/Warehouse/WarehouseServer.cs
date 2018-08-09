using System;
using System.Collections.Generic;
using System.Text;
using WarehouseCore;

namespace Lighthouse.Core.Apps.Warehouse
{
    public class WarehouserServer : LighthouseServiceBase
    {
		private readonly WarehouseCore.Warehouse warehouse = new WarehouseCore.Warehouse();

		protected override void OnStart()
		{
			
		}

		public IEnumerable<IShelf> GetShelves()
		{
			return null;
		}
	}
}
