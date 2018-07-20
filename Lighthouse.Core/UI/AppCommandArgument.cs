using System.Collections.Generic;

namespace Lighthouse.Core.UI
{
	public class AppCommandArgument
	{
		public AppCommand Command { get; private set; }
		public string ArgumentName { get; private set; }
		public IList<AppCommandArgumentHint> Hints { get; private set; }

		public AppCommandArgument(string argumentName, AppCommand command)
		{
			this.ArgumentName = argumentName;
			Hints = new List<AppCommandArgumentHint>();
			Command = command;
		}
	}
}