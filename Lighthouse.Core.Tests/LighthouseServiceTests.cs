using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Core.Tests
{
    public class LighthouseServiceTests : LighthouseTestsBase
	{
		public List<string> RecordedValues = new List<string>();

		public LighthouseServiceTests(ITestOutputHelper output) : base(output)
		{
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

			// init the app,
			app.Initialize(Container);

			// the app starts, and the startup tasks should run
			app.Start();
		}
    }

	public class TestApp : LighthouseServiceBase
	{
		public List<Action> StartupActions = new List<Action>();
		public List<Action> ScheduledTasks = new List<Action>();
	}
}
