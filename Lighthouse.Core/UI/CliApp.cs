using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lighthouse.Core.UI
{
	public class CliApp
	{
		public string Name { get; }
		public List<AppCommand> Commands { get; }
		public List<AppCommandValue> CommandValues { get; }
		private readonly Action<string> writeToConsole;
		private readonly Func<string> readFromConsole;
		private readonly Func<ConsoleKeyInfo> readKeyFromConsole;

		public CliApp(string name, Action<string> writeToConsole, Func<string> readFromConsole, Func<ConsoleKeyInfo> readKeyFromConsole)
		{
			Name = name;
			this.writeToConsole = writeToConsole;
			this.readFromConsole = readFromConsole;
			this.readKeyFromConsole = readKeyFromConsole;
			Commands = new List<AppCommand>();
			CommandValues = new List<AppCommandValue>();
		}

		public void Start(IList<string> args)
		{
			// get those args loaded up
			try
			{
				ValidateAndLoadArgs(args.Skip(1).ToList()); // skip the first argument for the app
			}
			catch(InvalidCommandException ice)
			{
				writeToConsole($"Invalid Argument: {ice.InvalidArgument}. {Environment.NewLine} Valid arguments are: {GetAllCommands()}");
			}

			QuitOnInput();
		}

		string GetAllCommands()
		{
			return string.Join(',', Commands);
		}

		void QuitOnInput()
		{
			writeToConsole("Press Any Key to quit...");
			var _ = readKeyFromConsole();			
		}

		public void ValidateAndLoadArgs(IList<string> args)
		{
			foreach (var arg in args) //.Select(a => a.Split('=')[0]))
			{
				if (TryParseArg(arg, out var val))
				{
					CommandValues.Add(new AppCommandValue { Command = val.Command, Value = val.CommandValue });
				}
				else
				{
					throw new InvalidCommandException(arg);
				}
			}
		}

		static (string argKey, string argValue) ParseArgs(string input)
		{
			var args = input.Split('=');
			return (args[0], args[1]);
		}

		bool TryParseArg(string arg, out (AppCommand Command, string CommandValue) value)
		{
			value = (null,null);
			var (argKey, argValue) = ParseArgs(arg);

			if (IsCommand(argKey))
			{				
				value = (Commands.FirstOrDefault(c => c.CommandName.Equals(argKey, StringComparison.OrdinalIgnoreCase)), argValue);
				return true;
			}
			
			return false;			
		}

		public bool IsCommand(string commandName)
		{
			return Commands.Any(c => c.CommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase));
		}
	}

	public static class CliAppBuilderExtensions
	{
		public static AppCommand AddCommand(this CliApp app, string commandName) //, Action<IAppCommands> )
		{
			if (app.IsCommand(commandName))
				throw new Exception("Command with that name is already added.");

			var command = new AppCommand(commandName, app);

			app.Commands.Add(command);
			return command;
		}

		public static AppCommand AddCommand(this AppCommand command, string commandName)
		{
			return command.App.AddCommand(commandName);			
		}

		public static AppCommandArgument AddArgument(this AppCommand command, string argumentName)
		{
			if (command.Arguments.Any(a => a.ArgumentName.ToLower() == argumentName.ToLower()))
				throw new Exception("Command with that name is already added.");

			var argument = new AppCommandArgument(argumentName, command);
			
			command.Arguments.Add(argument);
			return argument;
		}

		public static AppCommandArgument AddArgument(this AppCommandArgument arg, string commandName)
		{
			return arg.Command.AddArgument(commandName);
		}

		public static AppCommandArgument AddHint(this AppCommandArgument command, string label, string hintText)
		{
			if (command.Hints.Any(a => a.Label.ToLower() == label.ToLower()))
				throw new Exception("Command with that name is already added.");

			var hint = new AppCommandArgumentHint(label, hintText, command);

			command.Hints.Add(hint);
			return command;
		}		
	}

	public class AppCommandArgumentHint
	{
		public AppCommandArgument Command { get; private set; }
		public string Label { get; private set; }
		public string HintText { get; private set; }

		public AppCommandArgumentHint(string label, string hintText, AppCommandArgument command)
		{
			Label = label;
			HintText = hintText;
			Command = command;
		}
	}

	public class LighthouseTestCommands : IAppCommands
	{
	}

	public class LighthouseRunCommands : IAppCommands
	{
	}

	public class LighthouseDeployCommands : IAppCommands
	{
	}

	public interface IAppCommands
	{

	}
}
