using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Events;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Logging;
using Lighthouse.Core.Management;
using Lighthouse.Server.Management;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lighthouse.Server.Tests.Management
{
	public class ServerManagementRequestHandlerTests
	{
		[Fact]
		public async Task Handle_ValidRequest_InstallRequested()
		{
			var handler = new ServerManagementRequestHandler();
			var mockContainer = Substitute.For<ILighthouseServiceContainer>();
			var mockContext = Substitute.For<IManagementRequestContext>();
			mockContext.Container.Returns(mockContainer);

			var managementRequest = new ServerManagementRequest();
			managementRequest.RequestParameters.Add(ServerManagementRequest.RequestTypes.Install.Arguments.ServiceName, "testName");
			await handler.Handle(managementRequest.SerializeForManagementInterface(), mockContext);
			await mockContainer.ReceivedWithAnyArgs().EmitEvent(Arg.Any<IEvent>(), Arg.Any<ILighthouseLogSource>());
		}

		[Fact]
		public async Task Handle_InvalidRequest_NoAppName_InstallRequested()
		{
			var handler = new ServerManagementRequestHandler();
			var mockContainer = Substitute.For<ILighthouseServiceContainer>();
			var mockContext = Substitute.For<IManagementRequestContext>();
			mockContext.Container.Returns(mockContainer);

			var managementRequest = new ServerManagementRequest();
			Func<Task> func = () => handler.Handle(managementRequest.SerializeForManagementInterface(), mockContext);
			await func.Should().ThrowAsync<Exception>();
		}
	}
}