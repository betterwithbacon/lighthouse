using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lighthouse.Core.UI
{
	public class CliApp : IAppContext
	{
		public static class CommandPrompts
		{
			public const string PRESS_ANY_KEY_TO_QUIT = "Press Any Key to quit...";
			public const string MISSING_ARGUMENT = "Missing argument";
		}
		
		public string Name { get; }		
		public List<AppCommand> AvailableCommands { get; }		
		private readonly Action<string> WriteToConsole;
		private readonly Func<string> ReadFromConsole;
		private readonly Func<ConsoleKeyInfo> ReadKeyFromConsole;

		public AppCommand SelectedCommand { get; private set; }
		public List<AppCommandArgValue> SelectedCommandArgValues { get; private set; }
		private readonly ConcurrentDictionary<Type, object> Resources = new ConcurrentDictionary<Type, object>();

		public CliApp(
			string name, 
			Action<string> writeToConsole, 
			Func<string> readFromConsole, 
			Func<ConsoleKeyInfo> readKeyFromConsole,
			Func<bool, string, bool> onQuit = null)
		{
			Name = name;			
			this.WriteToConsole = writeToConsole;
			this.ReadFromConsole = readFromConsole;
			this.ReadKeyFromConsole = readKeyFromConsole;

			AvailableCommands = new List<AppCommand>();

			SelectedCommand = null;
			SelectedCommandArgValues = new List<AppCommandArgValue>();			
		}

		public void Log(string message)
		{
			WriteToConsole(message);
		}

		public async Task Start(IList<string> args)
		{
			// get those args loaded up
			try
			{
				// there's ALWAYS a first arg, because the command line, will always pass the app name
				ValidateAppName(args[0]);

				if (args.Count >= 2)
					LoadCommand(args[1]);
				else
					LoadCommand("");

				if (args.Count > 2)
					ValidateAndLoadCommandArgs(args.Skip(2).ToList()); // skip the app name, AND the app command

				var validationResults = ValidateCommand().Where(c => c.IsFatal);

				// if there are any fatal issues, just stop
				if(validationResults.Any())
				{	
					foreach(var issue in validationResults)
						WriteToConsole(issue.Message);					
				}
				else
				{
					// the app is valid, and ready to go, so do the commands that are connected
					await SelectedCommand.Execute(
							SelectedCommandArgValues
							.Select(acav => new KeyValuePair<string, string>(acav.Argument.ArgumentName, acav.Value))
							.ToImmutableDictionary()
					);
				}
			}
			catch(InvalidCommandException ice)
			{
				WriteToConsole($"Invalid Command: {ice.InvalidCommand}");
				WriteToConsole($"Valid commands are: {GetAllCommandsDescription()}");
			}
			catch(InvalidCommandArgumentException icae)
			{
				WriteToConsole($"Invalid Argument: {icae.InvalidArgument}");
				WriteToConsole($"Valid arguments are: {string.Join(',', SelectedCommand?.Arguments.Select(a => a.ToString())) }");
			}

			Quit();
		}

		public IEnumerable<AppCommandValidationResult> ValidateCommand()
		{
			foreach (var arg in SelectedCommand.Arguments)
			{
				// if an arg is required, and we don't have a value for it, then cvcreat a failure for it
				if(arg.IsRequired && !SelectedCommandArgValues.Any(argVal => argVal.Argument == arg))
				{
					yield return new MissingRequiredArgValidationResult(arg);
				}
			}
		}

		public void Finish(string message = "Done!")
		{
			Quit(true, message);
		}

		public AppCommand GetCommand(string commandName)
		{
			return AvailableCommands.FirstOrDefault(a => a.CommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase));
		}

		private void LoadCommand(string commandName)
		{
			if(IsCommand(commandName))
			{
				SelectedCommand = AvailableCommands.First(ac => ac.CommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase));
			}
			else
				throw new InvalidCommandException(commandName);
		}

		private void ValidateAppName(string appName)
		{
			if (!Name.ToLower().Equals(appName, StringComparison.OrdinalIgnoreCase))
				throw new ApplicationException($"AppName doesn't match. Found: {appName} Expected: {Name}");
		}

		string GetAllCommandsDescription()
		{
			return string.Join(',', AvailableCommands);
		}

		public void Quit(bool isFatal = false, string quitMessage = null)
		{
			if (quitMessage != null)
				WriteToConsole(quitMessage);
			
			if (isFatal)
			{
				Environment.Exit(-1);				
			}
			else
			{
				WriteToConsole(CommandPrompts.PRESS_ANY_KEY_TO_QUIT);
				var _ = ReadKeyFromConsole();
			}
		}

		private void ValidateAndLoadCommandArgs(IList<string> args)
		{
			foreach (var arg in args)
			{
				if (TryParseArg(arg, SelectedCommand, out var val))
				{
					SelectedCommandArgValues.Add(new AppCommandArgValue { Argument = val.CommandArgument, Value = val.ArgValue });
				}
				else
				{
					throw new InvalidCommandArgumentException(arg);
				}
			}
		}

		static (string argKey, string argValue) ParseArgs(string input)
		{
			if (string.IsNullOrEmpty(input))
				return ("", "");

			var args = input.Split('=');

			if (args.Count() == 1)
				return (args[0], "");
			else
				return (args[0], args[1]);
		}

		bool TryParseArg(string arg, AppCommand command, out (AppCommandArgument CommandArgument, string ArgValue) value)
		{
			value = (null,null);

			if (command == null)
				throw new ArgumentNullException(nameof(command));

			var (argKey, argValue) = ParseArgs(arg);

			if (command.IsValidArgument(argKey))
			{
				value = (command.Arguments.FirstOrDefault(c => c.ArgumentName.Equals(argKey, StringComparison.OrdinalIgnoreCase)), argValue);
				return true;
			}
			
			return false;			
		}

		public bool IsCommand(string commandName)
		{
			return AvailableCommands.Any(c => c.CommandName.Equals(commandName, StringComparison.OrdinalIgnoreCase));
		}

		public void InvalidArgument(string argName, string reason)
		{
			Quit(true, $"Invalid argument {argName}: {reason}");
		}

		public void Fault(string errorMessage)
		{
			Quit(true, errorMessage);
		}

		public T GetResource<T>()
			where T : class
		{
			if(Resources.TryGetValue(typeof(T), out var val))
			{
				return val as T;
			}
			return default;
		}

		protected void AddResource(object resource)
		{
			// TODO: this is just laziness. We need to make resolution a bit more complex
			if (Resources[resource.GetType()] != null)
				throw new InvalidOperationException("Type of resource already inserted");

			Resources.TryAdd(resource.GetType(), resource);
		}
	}

	public static class CliAppBuilderExtensions
	{
		public static AppCommand AddCommand<T>(this CliApp app, string commandName)
			where T : IAppCommandHandler
		{
			if (app.IsCommand(commandName))
				throw new Exception("Command with that name is already added.");

			var command = new AppCommand(commandName, app)
			{
				ExecutionActionType = typeof(T),
			};

			app.AvailableCommands.Add(command);
			return command;
		}

		public static AppCommand AddCommand(this CliApp app, string commandName, Action<IDictionary<string, string>> executionAction = null)
		{
			if (app.IsCommand(commandName))
				throw new Exception("Command with that name is already added.");

			var command = new AppCommand(commandName, app)
			{
				ExecutionAction = executionAction
			};

			app.AvailableCommands.Add(command);
			return command;
		}

		public static AppCommand AddCommand(this AppCommand command, string commandName, Action<IDictionary<string, string>> executionAction = null)
		{
			return command.App.AddCommand(commandName, executionAction);			
		}

		public static AppCommandArgument AddArgument(this AppCommand command, string argumentName, bool isRequired = false)
		{
			if (command.Arguments.Any(a => a.ArgumentName.ToLower() == argumentName.ToLower()))
				throw new Exception("Command with that name is already added.");

			var argument = new AppCommandArgument(argumentName, command, isRequired);
			
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

	public interface IAppCommandHandler
	{
		Task Handle(IDictionary<string, string> argValues, IAppContext context);		
	}
}
