using System.Threading.Tasks;

namespace Lighthouse.Core.Management
{
	public interface IManagementRequestHandler
	{
		Task<object> Handle(string rawRequestPayload, IManagementRequestContext requestContext);
	}
}