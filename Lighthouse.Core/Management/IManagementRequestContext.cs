namespace Lighthouse.Core.Management
{
	public interface IManagementRequestContext
	{
		ILighthouseServiceContainer Container { get; }
	}
}