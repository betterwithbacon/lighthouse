using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Management;
using Lighthouse.Server.Management;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Lighthouse.Server.Tests.Management
{
	public class ServerManagementRequestHandlerTests
	{
		[Fact]
		public void Handle_ValidRequest_InstallRequested()
		{
			var handler = new ServerManagementRequestHandler();
			var mockContainer = Substitute.For<ILighthouseServiceContainer>();
			var mockContext = Substitute.For<IManagementRequestContext>();
			mockContext.Container.Returns(mockContainer);


			var response = handler.Handle("appName", mockContext) as string;
			response.Should().NotBeNull();
		}
	}
}
