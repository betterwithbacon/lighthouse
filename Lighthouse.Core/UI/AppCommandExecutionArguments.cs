using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lighthouse.Core.UI
{
    public class AppCommandExecution
    {
		public CliApp App { get; private set; }
		public AppCommand InvokedCommand { get; private set; }
		public List<AppCommandArgValue> ArgValues { get; set; }

		public AppCommandExecution(CliApp app, AppCommand invokedCommand, IList<AppCommandArgValue> argValues)
		{
			App = app;
			InvokedCommand = invokedCommand;
			ArgValues = argValues.ToList();
		}
	}

	public static class ArgValueListExtensions
	{
		public static string FirstOrDefaultCommandArgValue(this AppCommandExecution commandExecution, string argumentName)
		{
			return commandExecution?
				.ArgValues?
				.FirstOrDefault(
					(acav) => 
						acav.Argument
							.ArgumentName
							.Equals(argumentName, StringComparison.OrdinalIgnoreCase)
				)?.Value;
		}
	}

}
