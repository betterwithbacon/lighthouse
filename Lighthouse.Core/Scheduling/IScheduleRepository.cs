using System;
using System.Collections.Generic;
using System.Text;

namespace Lighthouse.Core.Scheduling
{
	public interface IScheduleHistoryRepository : ILighthouseService
	{
		DateTime? GetLastRunDate(Schedule schedule);
	}
}
