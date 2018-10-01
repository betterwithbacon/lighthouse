namespace Lighthouse.Core.Configuration.ServiceDiscovery
{
	public interface ILighthouseServiceDescriptor
	{
		string Name { get; }
		string Type { get; }
		string Alias { get; }
	}
}