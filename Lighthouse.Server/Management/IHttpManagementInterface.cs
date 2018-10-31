namespace Lighthouse.Server.Management
{
	public interface IHttpManagementInterface : ILighthouseManagementInterface
	{
		int Port { get; }
	}
}