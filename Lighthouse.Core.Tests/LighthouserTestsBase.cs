using Lighthouse.Server;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace Lighthouse.Core.Tests
{
    public abstract class LighthouseTestsBase
    {
		protected readonly List<string> Messages = new List<string>();
		protected readonly LighthouseServer Container;
		protected ITestOutputHelper Output { get; }

		public LighthouseTestsBase(ITestOutputHelper output)
		{
			Output = output;

			Container = new LighthouseServer(localLogger:(message) => {
				Messages.Add(message); Output.WriteLine(message);
			});
		}
	}
}
