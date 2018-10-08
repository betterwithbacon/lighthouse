using FluentAssertions;
using Lighthouse.Core.IO;
using Lighthouse.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Core.Tests.IO
{
	public class WindowsFileSystemProviderTests : LighthouseTestsBase
	{
		protected readonly WindowsFileSystemProvider Provider;

		public WindowsFileSystemProviderTests(ITestOutputHelper output)
			: base(output)
		{
			Provider = new WindowsFileSystemProvider(Container.WorkingDirectory, Container);
			Container.RegisterResourceProvider(Provider);
		}

		[Fact]
		public async Task WriteToFileSystem_FileWritesCorrectly()
		{
			var fileName = "testfile1.txt";
			var fileData = "test";
			Provider.WriteToFileSystem(fileName, Encoding.UTF8.GetBytes(fileData));

			// TODO: the issue with thnis method of verification, is that it's contingent on Read working correctly.
			// Probably should use: File.OpenRead("FILE") to verify it, as it's an alternative method.
			Encoding.UTF8.GetString(await Provider.ReadFromFileSystem(fileName)).Should().Be(fileData);
		}

		[Fact]
		public async Task ReadFromFileSystem_FileReadsCorrectly()
		{
			var fileName = "testfile1.txt";
			UnicodeEncoding.UTF8.GetString(await Provider.ReadFromFileSystem(fileName)).Should().Be("test");
		}

		[Fact]
		public void FileExists_IsPerceivedCorrectly()
		{
			var fileName = "testfile1.txt";
			Provider.FileExists(fileName).Should().BeTrue();			
		}
	}
}
