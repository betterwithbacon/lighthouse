using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Utils;
using Lighthouse.Server;
using Xunit;
using Xunit.Abstractions;

namespace Lighthouse.CLI.Tests
{
    public class SimulationTests
    {
        public SimulationTests(ITestOutputHelper output)
        {
            Output = output;
        }

        public ITestOutputHelper Output { get; }


        [Fact]
        public void LargeSimulation()
        {
            // all simulatiln traffic will flow through this
            var network = new VirtualNetwork();

            // spin up 3 nodes (to make it spicey)
            // these nodes represent, machines starting up, and joining a local network
            // we have to assume the networking is configured properly, they have IP addresses,
            // also assigned by the network, that they may or may not be aware of
            var consoleLog = new List<string>();
            var currentConsole = "";

            void writeDelegate(string text)
            {
                currentConsole = text;
                Output.WriteLine(text);
                Console.WriteLine(text);
                consoleLog.Add(text);
            };

            string readDelegate() => Console.ReadLine();


            var container1Name = "container1";
            var container1 = new LighthouseServer(container1Name);
            container1.AddLogger((log) => writeDelegate($"{container1Name}: {log}"));
            container1.RegisterResource(network);

            var container2Name = "container2";
            var container2 = new LighthouseServer(container2Name);
            container1.AddLogger((log) => writeDelegate($"{container2Name}: {log}"));
            container2.RegisterResource(network);

            var container3Name = "container3";
            var container3 = new LighthouseServer(container3Name);
            container1.AddLogger((log) => writeDelegate($"{container3Name}: {log}"));
            container3.RegisterResource(network);

            // now we assume a user
            var user = new User(writeDelegate, readDelegate, network);

            // the user, literally can only do what a person can do, and vice versa
            user.Type($"lighthouse inspect --where {network.ResolveUri(container1)}");

            // this should return a simple status message
            currentConsole.Should().Contain(StatusRequestHandler.GLOBAL_VERSION_NUMBER);
            currentConsole.Should().Contain(container1Name);
        }
    }

    internal class User
    {
        private readonly Action<string> WriteDelegate;
        private readonly Func<string> ReadDelegate;
        public CommandLineRunner Runner { get; }
        public VirtualNetwork Network { get; }
        public TypeFactory TypeFactory { get; }

        public User(Action<string> writeDelegate, Func<string> readDelegate, VirtualNetwork network)
        {
            WriteDelegate = writeDelegate;
            ReadDelegate = readDelegate;
            Network = network;
            TypeFactory = new TypeFactory();
            TypeFactory.Register<INetworkProvider>(() => Network);
            Runner = new CommandLineRunner(WriteDelegate, ReadDelegate, TypeFactory);
        }

        public void Type(string text)
        {
            Runner.Run(text.Split(" ").Skip(1));
        }

        public string Read() => ReadDelegate();
    }
}
