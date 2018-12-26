using System;

namespace Lighthouse.Core.UI
{
	/// <summary>
	/// Represents an abstraction between the call of a CLI request handler, and mutating the state of the application
	/// This is done, because the CLI app state, might also need to be reflected in other contexts, and this context gives a single point for that sharing to occur
	/// </summary>
	public interface IAppContext
	{
		void Fault(string errorMessage);

		void InvalidArgument(string argName, string reason);

		void Quit(bool isFatal, string message);

		void Finish(string message);

		void Log(string message);
		T GetResource<T>()
			where T : class;
	}
}