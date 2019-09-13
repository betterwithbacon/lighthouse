using FluentAssertions;
using Lighthouse.Core.IO;
using Lighthouse.Server;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Apps.Database.Tests
{
    public class KVDTests
    {
        protected readonly List<string> Messages = new List<string>();
        protected readonly LighthouseServer Container;
        protected ITestOutputHelper Output { get; }

        public KVDTests(ITestOutputHelper output)
        {
            Output = output;

            Container = new LighthouseServer(localLogger: (message) => {
                Messages.Add(message); Output.WriteLine(message);
            });
        }

        [Fact]
        public async Task Store_WhenStored_FileIsThere()
        {
            var mockFileSystem = NSubstitute.Substitute.For<IFileSystemProvider>();
            Container.RegisterResourceProvider(mockFileSystem);
            Container.Start();

            var db = new KVD();
            await Container.Launch(db);
            
            // write file
            await db.Store("global", "key", "value");

            mockFileSystem.ReceivedWithAnyArgs().WriteToFileSystem(Arg.Any<string>(), Arg.Any<byte[]>());
        }

        [Fact]
        public async Task Retrieve_WhenStored_DataIsReturned()
        {
            var mockFileSystem = NSubstitute.Substitute.For<IFileSystemProvider>();
            Container.RegisterResourceProvider(mockFileSystem);
            Container.Start();

            var db = new KVD();
            await Container.Launch(db);
            var payload = "value";
            var partition = "partition";
            var key = "key";
            await db.Store(partition, key, payload);
            var retrievedPayload = await db.Retrieve(partition, key);
            retrievedPayload.Should().Be(payload);
        }
    }
}
