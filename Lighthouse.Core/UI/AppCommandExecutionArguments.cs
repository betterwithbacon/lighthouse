using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lighthouse.Core.UI
{
    public class AppCommandExecution
    {
		public string InvokedCommand { get; }
		public Dictionary<string, string> ArgValues { get; }

		public AppCommandExecution(CliApp app, string invokedCommand, Dictionary<string, string> argValues)
		{
			InvokedCommand = invokedCommand;
			ArgValues = argValues;
		}
	}

	//public static class ArgValueListExtensions
	//{
	//	public static string FirstOrDefaultCommandArgValue(this AppCommandExecution commandExecution, string argumentName)
	//	{
	//		return commandExecution?
	//			.ArgValues?
	//			.FirstOrDefault(
	//				(acav) => 
	//					acav.Argument
	//						.ArgumentName
	//						.Equals(argumentName, StringComparison.OrdinalIgnoreCase)
	//			)?.Value;
	//	}
	//}

}
