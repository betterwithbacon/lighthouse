using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Utils;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.CLI.Tests
{
    public class CliTests
    {
        public CliTests(ITestOutputHelper output)
        {
            Output = output;
        }

        public ITestOutputHelper Output { get; }

        [Fact]
        public void Run_WhatWhere_Works()
        {
            var where = "http://127.0.0.1";
            var what = "ping";
            var typeFactory = new TypeFactory();
            
            // just create a dumb network, that will let the console run and fail expectedly
            var virtualNetwork = new VirtualNetwork();
            typeFactory.Register<INetworkProvider>(() => virtualNetwork);

            var pingContainer = Substitute.For<ILighthouseServiceContainer>();
            virtualNetwork.Register(pingContainer, where.ToUri());

            RemoteAppRunRequest receivedRequest = null;
            var returnValue = new RemoteAppRunHandle(Guid.NewGuid().ToString());

            pingContainer.HandleRequest<RemoteAppRunRequest, RemoteAppRunHandle>(Arg.Do<RemoteAppRunRequest>(r => receivedRequest = r)).Returns(returnValue);
            
            var consoleWrites = new List<string>();
            var runner = new CommandLineRunner((log) => {
                consoleWrites.Add(log);
                Output.WriteLine(log);
            }, () => "no_console_reads", typeFactory);

            var returnCode = runner.Run($"lighthouse run --what {what} --where {where}".Split(" ").Skip(1));

            receivedRequest.Should().NotBeNull();
            receivedRequest.What.Should().Be(what);
        }

        //[Fact]
        //public void Run_d()
        //{
        //    var where = "http://127.0.0.1";
        //    var what = "ping";

        //    // just create a dumb network, that will let the console run and fail expectedly
        //    var virtualNetwork = new VirtualNetwork();
        //    var pingContainer = Substitute.For<ILighthouseServiceContainer>();
        //    virtualNetwork.Register(pingContainer, where.ToUri());

        //    RemoteAppRunRequest receivedRequest = null;
        //    var returnValue = new RemoteAppRunHandle(Guid.NewGuid().ToString());

        //    pingContainer.HandleRequest<RemoteAppRunRequest, RemoteAppRunHandle>(Arg.Do<RemoteAppRunRequest>(r => receivedRequest = r)).Returns(returnValue);

        //    var consoleWrites = new List<string>();
        //    var runner = new CommandLineRunner((log) => {
        //        consoleWrites.Add(log);
        //        Output.WriteLine(log);
        //    }, () => "no_console_reads", virtualNetwork);

        //    var returnCode = runner.Run($"lighthouse run --what {what}--where {where}".Split(" ").Skip(1));

        //    receivedRequest.Should().NotBeNull();
        //    receivedRequest.What.Should().Be(what);
        //}
    }
}
