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
        public void Inspect_127_0_0_1_Works()
        {
            var where = "http://127.0.0.1";
            var (_, Succeeded, _) = Inspect(where);
            Succeeded.Should().BeTrue();
        }

        [Fact]
        public void Run_Ping_127_0_0_1_Works()
        {
            var where = "http://127.0.0.1";
            var what = "ping";            
            Run(what, where);
        }

        [Fact]
        public void Run_Ping_127_0_0_1_LogsWritten()
        {
            var where = "http://127.0.0.1";
            var what = "ping";
            var (logs, _, _) = Run(what, where);
            var allLogs = string.Join(" ", logs);
            foreach (var expected in new string[]{ "Request", RemoteAppRunStatus.Succeeded})
            {
                allLogs.Contains(expected).Should().BeTrue();
            }
        }

        [Fact(Skip = "this won't work until we actually have a way for exceptions to be sent back from remote")]
        public void Run_Invalid_127_0_0_1_Fails()
        {
            var where = "http://127.0.0.1";
            var what = "invalid";
            Run(what, where).Succeeded.Should().BeFalse();
        }

        private (List<string> Log, bool Succeeded, Exception Error) Inspect(string where)
        {
            var command = $"lighthouse inspect --where {where}";

            var virtualNetwork = new VirtualNetwork();

            var typeFactory = new TypeFactory();
            typeFactory.Register<INetworkProvider>(() => virtualNetwork);

            var pingContainer = Substitute.For<ILighthouseServiceContainer>();
            virtualNetwork.Register(pingContainer); //, where.ToUri());

            RemoteAppRunRequest receivedRequest = null;
            var returnValue = new RemoteAppRunHandle(Guid.NewGuid().ToString());

            pingContainer.HandleRequest<RemoteAppRunRequest, RemoteAppRunHandle>(Arg.Do<RemoteAppRunRequest>(r => receivedRequest = r)).Returns(returnValue);

            var consoleWrites = new List<string>();
            var runner = new CommandLineRunner((log) => {
                consoleWrites.Add(log);
                Output.WriteLine(log);
            }, () => "no_console_reads", typeFactory);

            try
            {
                var returnCode = runner.Run(command.Split(" ").Skip(1));
                receivedRequest.Should().NotBeNull();
                
                return (consoleWrites, true, null);
            }
            catch (Exception e)
            {
                return (consoleWrites, false, e);
            }
        }

        private (List<string> Log, bool Succeeded, Exception Error) Run(string what, string where)
        {
            var command = $"lighthouse run --what {what} --where {where}";

            var typeFactory = new TypeFactory();
            
            // just create a dumb network, that will let the console run and fail expectedly
            var virtualNetwork = new VirtualNetwork();
            typeFactory.Register<INetworkProvider>(() => virtualNetwork);

            var pingContainer = Substitute.For<ILighthouseServiceContainer>();
            virtualNetwork.Register(pingContainer); //, where.ToUri());

            RemoteAppRunRequest receivedRequest = null;
            var returnValue = new RemoteAppRunHandle(Guid.NewGuid().ToString());

            pingContainer.HandleRequest<RemoteAppRunRequest, RemoteAppRunHandle>(Arg.Do<RemoteAppRunRequest>(r => receivedRequest = r)).Returns(returnValue);
            
            var consoleWrites = new List<string>();
            var runner = new CommandLineRunner((log) => {
                consoleWrites.Add(log);
                Output.WriteLine(log);
            }, () => "no_console_reads", typeFactory);

            try
            {
                var returnCode = runner.Run(command.Split(" ").Skip(1));
                receivedRequest.Should().NotBeNull();

                // the response just roundtrips the request
                receivedRequest.What.Should().Be(what);
                return (consoleWrites, true, null);
            }
            catch(Exception e)
            {
                return (consoleWrites, false, e);
            }
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
