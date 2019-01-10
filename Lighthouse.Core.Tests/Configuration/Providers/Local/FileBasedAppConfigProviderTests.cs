using FluentAssertions;
using Lighthouse.Core.Configuration.Formats.YAML;
using Lighthouse.Core.Configuration.Providers.Local;
using Lighthouse.Core.Configuration.ServiceDiscovery;
using Lighthouse.Core.IO;
using Lighthouse.Core.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Core.Tests.Configuration.Providers.Local
{
	public class FileBasedAppConfigProviderTests
	{
		private readonly ITestOutputHelper Output;

		public FileBasedAppConfigProviderTests(ITestOutputHelper output)
		{
			Output = output;
		}

		[Fact]
		public void FileIsReadAndSavesCorrectly()
		{
			// dest
			var destFilePath = Path.Combine(Environment.CurrentDirectory, LighthouseYamlBaseConfig.DEFAULT_CONFIG_FILENAME);

			// copy the test path to the read location
			File.Copy(
				Path.Combine("C:\\development\\lighthouse\\", LighthouseYamlBaseConfig.DEFAULT_CONFIG_FILENAME),
				destFilePath,
				true);

			byte[] data = null;
			var fileName = "";

			var mockLighthouse = Substitute.For<ILighthouseServiceContainer>();
			var mockFileSystem = Substitute.For<IFileSystemProvider>();
			
			mockFileSystem.WriteToFileSystem(
				Arg.Do<string>((file) => fileName = file), 
				Arg.Do<byte[]>((dat) => data = dat));
			mockLighthouse.GetFileSystemProviders().Returns(new[] { mockFileSystem });
			
			var testVersion = "10.0";
			var testMaxThreadCount = 16;
			var provider = new FileBasedAppConfigProvider(mockLighthouse, destFilePath, version: testVersion, maxThreadCount: testMaxThreadCount);
			var testApp = new ServiceLaunchRequest("testApp");
			provider.AddServiceLaunchRequest(testApp);

			provider.Save();

			fileName.Should().Be(destFilePath);
			data.Should().NotBeNull();
			var decodedData = Encoding.UTF8.GetString(data);
			decodedData.Should().NotBeNullOrEmpty();
		}

		[Fact]
		[Trait("Category","Integration")]
		public void FileIsReadAndSavesToFileSystemCorrectly()
		{
			// dest
			var destFilePath = Path.Combine(Environment.CurrentDirectory, LighthouseYamlBaseConfig.DEFAULT_CONFIG_FILENAME);

			// copy the test path to the read location
			File.Copy(
				Path.Combine("C:\\development\\lighthouse\\", LighthouseYamlBaseConfig.DEFAULT_CONFIG_FILENAME),
				destFilePath,
				true);

			byte[] data = null;
			
			var mockLighthouse = Substitute.For<ILighthouseServiceContainer>();
			// LogLevel level, LogType logType,  ILighthouseLogSource sender, string message = null, Exception exception = null, bool emitEvent = true
			mockLighthouse.Log(Arg.Any<LogLevel>(), Arg.Any<LogType>(), Arg.Any<ILighthouseLogSource>());
			var filesystem = new WindowsFileSystemProvider(Environment.CurrentDirectory, mockLighthouse);					
			mockLighthouse.GetFileSystemProviders().Returns(new[] { filesystem });

			var testVersion = "10.0";
			var testMaxThreadCount = 16;
			var provider = new FileBasedAppConfigProvider(mockLighthouse, destFilePath, version: testVersion, maxThreadCount: testMaxThreadCount);
			var testApp = new ServiceLaunchRequest("testApp");
			provider.AddServiceLaunchRequest(testApp);

			provider.Save();

			
			var decodedData = Encoding.UTF8.GetString(data);
			decodedData.Should().NotBeNullOrEmpty();
		}
	}
}
