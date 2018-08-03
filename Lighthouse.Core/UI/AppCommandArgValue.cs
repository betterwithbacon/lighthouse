namespace Lighthouse.Core.UI
{
	public class AppCommandArgValue
	{
		public AppCommandArgument Argument { get; internal set; }
		public string Value { get; internal set; }

		public override string ToString()
		{
			return $"{Argument}:{Value ?? "<none>"}";
		}
	}
}