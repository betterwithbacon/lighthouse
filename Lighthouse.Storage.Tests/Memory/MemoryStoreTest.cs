﻿using FluentAssertions;
using Lighthouse.Core.Storage;
using Lighthouse.Server;
using Lighthouse.Storage.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lighthouse.Storage.Tests.Memory
{
    public class MemoryStoreTest
    {
        [Fact]
        public async Task GetManifests_ReturnsAllRecords()
        {
            var container = new LighthouseServer();
            var memoryStore = new InMemoryKeyValueStore();
            memoryStore.Initialize(container);

            var key = "test";
            var payload = "payload";

            //var policies = new ConcurrentBag<StoragePolicy>();
            //policies.Add(StoragePolicy.Ephemeral);
            memoryStore.Store(StorageScope.Global, key, payload);//, policies);
            var manifests = await memoryStore.GetManifests(StorageScope.Global);

            manifests.Count().Should().Be(1);
            manifests.First().Key.Should().Be(key);
        }
    }
}