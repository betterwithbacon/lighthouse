namespace Lighthouse.Core.Management
{
	public class ManagementInterfaceResponse
	{
		public bool WasSuccessful { get;  }
		public string Message { get; }

		public ManagementInterfaceResponse(bool wasSuccessful, string message)
		{
			WasSuccessful = wasSuccessful;
			Message = message;
		}
	}
}