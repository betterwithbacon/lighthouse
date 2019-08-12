using Lighthouse.Core.IO;
using Lighthouse.Server;
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
            var db = new KVD();

            var mockFileSystem = NSubstitute.Substitute.For<IFileSystemProvider>();
            Container.RegisterResourceProvider(mockFileSystem);

            // write file
            await db.Store("global", "key", "value");
            
            mockFileSystem.
        }

        [Fact]
        public void Retrieve_WhenStored_DataIsReturned()
        {

        }
    }
}
