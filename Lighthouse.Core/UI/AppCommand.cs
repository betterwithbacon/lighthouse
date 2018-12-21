using System;
using System.Collections.Generic;
using System.Linq;

namespace Lighthouse.Core.UI
{
	public class AppCommand
	{
		public CliApp App { get; private set; }

		public IList<AppCommandArgument> Arguments { get; private set; }

		public string CommandName { get; private set; }

		public Action<IDictionary<string, string>> ExecutionAction { get; internal set; }
		public Type ExecutionActionType { get; internal set; }

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

		public bool IsValidArgument(string argKey)
		{
			return Arguments.Any(c => c.ArgumentName.Equals(argKey, StringComparison.OrdinalIgnoreCase));
		}

		public void Execute(IDictionary<string, string> argValues)
		{
			if (ExecutionActionType != null)
			{
				var executor = Activator.CreateInstance(ExecutionActionType) as IAppCommandHandler;
				executor?.Handle(argValues, App);
			}
			else
			{
				ExecutionAction?.Invoke(argValues);
			}
		}
	}

	public class AppCommandValidationResult
	{
		public string Message { get; private set; }
		public bool IsValid { get; private set; }		
		public bool IsFatal { get; internal set; }

		public AppCommandValidationResult(string message, bool isValid, bool isFatal)
		{
			IsValid = isValid;
			Message = message;
			IsFatal = isFatal;
		}
	}

	public class MissingRequiredArgValidationResult : AppCommandValidationResult
	{
		public MissingRequiredArgValidationResult(AppCommandArgument argument)
			: base($"Required argument was missing: {argument.ArgumentName}",false, true)
		{
		}
	}

	//public enum AppCommandValidationResultType
	//{
	//	Valid,
	//	Failed,
	//	Informational,
	//	Obsolete
	//}
}