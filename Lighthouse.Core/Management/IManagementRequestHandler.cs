namespace Lighthouse.Core.Management
{
	public interface IManagementRequestHandler
	{
		object Handle(string rawRequestPayload, IManagementRequestContext requestContext);
	}
}