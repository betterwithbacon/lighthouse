using System;
using System.Collections.Generic;

namespace Lighthouse.Core.UI
{
	public class AppCommand
	{
		public CliApp App { get; private set; }

		public IList<AppCommandArgument> Arguments { get; private set; }

		public string CommandName { get; private set; }

		public AppCommand(string commandName, CliApp app)
		{
			if (string.IsNullOrEmpty(commandName))
				throw new ArgumentException("CommandName can't be empty.", nameof(commandName));

			App = app;
			CommandName = commandName;
			Arguments = new List<AppCommandArgument>();
		}

		public override string ToString()
		{
			return CommandName;
		}
	}
}