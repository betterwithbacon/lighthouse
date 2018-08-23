using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Lighthouse.Core.Tests
{
    public class LighthouseServiceTests
    {
		public List<string> RecordedValues = new List<string>();

		private class TestApp : LighthouseServiceBase
		{
			public List<Action> StartupActions = new List<Action>();
			public List<Action> ScheduledTasks = new List<Action>();

			//public override void OnBuild(LighthouseServiceBuilder serviceBuilder)
			//{
			//	foreach (var action in StartupActions)
			//		AddStartupTask(action);

			//	foreach (var action in ScheduledTasks)
			//		AddScheduledTask(action);
			//}
		}

		[Fact]
		public void StartupTaskTests()
		{
			var expectedValues = new[] { "hit1", "hit2", "hit3", "hit4", "hit5" };
			TestApp app = new TestApp();
			app.StartupActions.Add(
				() => RecordedValues.Add(expectedValues[0])
			);

			app.StartupActions.Add(
				() => RecordedValues.Add(expectedValues[1])
			);

			app.ScheduledTasks.Add(() => RecordedValues.Add(expectedValues[2]) );

			// the app starts, and the startup tasks should run
			app.Start();
		}
    }
}
