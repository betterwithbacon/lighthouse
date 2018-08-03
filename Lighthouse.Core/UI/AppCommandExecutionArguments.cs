using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lighthouse.Core.UI
{
    public class AppCommandExecutionArguments
    {
		public CliApp App { get; private set; }
		public AppCommand InvokedCommand { get; private set; }
		public List<AppCommandArgValue> ArgValues { get; set; }

		public AppCommandExecutionArguments(CliApp app, AppCommand invokedCommand, IList<AppCommandArgValue> argValues)
		{
			App = app;
			InvokedCommand = invokedCommand;
			ArgValues = argValues.ToList();
		}
	}
}
