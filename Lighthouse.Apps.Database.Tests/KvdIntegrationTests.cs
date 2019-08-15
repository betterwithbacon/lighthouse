using FluentAssertions;
using Lighthouse.Core.IO;
using Lighthouse.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.Apps.Database.Tests
{
    public class KvdIntegrationTests
    {
        protected readonly List<string> Messages = new List<string>();

        public ITestOutputHelper Output { get; }

        protected readonly LighthouseServer Container;

        public KvdIntegrationTests(ITestOutputHelper output)
        {
            Output = output;

            Container = new LighthouseServer(localLogger: (message) => {
                Messages.Add(message); Output.WriteLine(message);
            });
        }

        [Fact]
        public async Task FullLifeCycle_RoundTripped()
        {
            Container.RegisterResourceProvider(
                new WindowsFileSystemProvider("C:\\Development\\Lighthouse", Container)
            );
            Container.Start();

            var db = new KVD();
            Container.Launch(db);

            var payload = "value";
            var partition = "partition";
            var key = "key";
            await db.Store(partition, key, payload);
            db.WriteToFileSystem();
        }


        [Fact]
        public async Task FullLifeCycle_Perf()
        {
            Container.RegisterResourceProvider(
                new WindowsFileSystemProvider("C:\\Development\\Lighthouse", Container)
            );
            Container.Start();

            var db = new KVD();
            Container.Launch(db);

            var partition = "partition";
            var recCount = 100;
            var timer = Stopwatch.StartNew();
            foreach (var index in Enumerable.Range(0, recCount))
            {
                await db.Store(partition, $"key_{index}", index.ToString());
            }
            db.Stop();
            timer.Stop();
            Output.WriteLine($"{timer.ElapsedMilliseconds}ms");

            db = null;

            db = new KVD();
            Container.Launch(db);
            Thread.Sleep(100);
            db.Entries.Count().Should().Be(recCount);

        }
    }
}
