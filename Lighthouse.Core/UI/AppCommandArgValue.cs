namespace Lighthouse.Core.UI
{
	public class AppCommandArgValue
	{
		public AppCommandArgument Argument { get; set; }
		public string Value { get; set; }

		public override string ToString()
		{
			return $"{Argument}:{Value ?? "<none>"}";
		}
	}
}