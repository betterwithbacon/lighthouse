﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.IO;
using Lighthouse.Core.Storage;
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
            
            
            void writeDelegate(string text)
            {
                Output.WriteLine(text);                
            };

            string readDelegate() => Console.ReadLine();


            var container1Name = "container1";
            var container1 = new LighthouseServer(container1Name);
            container1.AddLogger((log) => writeDelegate($"{container1Name} LOG: {log}"));
            container1.RegisterResource(network);

            var container2Name = "container2";
            var container2 = new LighthouseServer(container2Name);
            container2.AddLogger((log) => writeDelegate($"{container2Name} LOG: {log}"));
            container2.RegisterResource(network);

            var container3Name = "container3";
            var container3 = new LighthouseServer(container3Name);
            container3.AddLogger((log) => writeDelegate($"{container3Name} LOG: {log}"));
            container3.RegisterResource(network);

            // now we assume a user
            // the user, literally can only do what a person can do, and vice versa
            var user = new User(writeDelegate, readDelegate, network);

            // can we reach each server
            user.ActAndAssert(
                act => act.Type($"lighthouse inspect --where {network.ResolveUri(container1)}"),
                (console) =>
                {
                    console.Should().Contain(StatusRequestHandler.GLOBAL_VERSION_NUMBER);
                    console.Should().Contain(container1Name);
                }
            );

            user.ActAndAssert(
               act => act.Type($"lighthouse inspect --where {network.ResolveUri(container2)}"),
               (console) =>
               {
                   console.Should().Contain(StatusRequestHandler.GLOBAL_VERSION_NUMBER);
                   console.Should().Contain(container2Name);
               }
           );

            user.ActAndAssert(
               act => act.Type($"lighthouse inspect --where {network.ResolveUri(container3)}"),
               (console) =>
               {
                   console.Should().Contain(StatusRequestHandler.GLOBAL_VERSION_NUMBER);
                   console.Should().Contain(container3Name);
               }
           );

            // the servers are all active and responding, now see them respond to actual pings
           user.ActAndAssert(
               act => act.Type($"lighthouse run --what ping --where {network.ResolveUri(container3)}"),
               (console) =>
               {
                   console.Should().Contain(RemoteAppRunStatus.Succeeded);
               }
           );

           user.ActAndAssert(
                 act => act.Type($"lighthouse inspect --what services --where {network.ResolveUri(container3)}"),
                consoleMultiLine: (console) =>
                {
                    console.Any(s => s.Contains("ping")).Should().BeTrue();
                }
           );

            // the ping should have run, and the result should have been written to this servers log.
            // read the log
            // TODO: find way to add this back in a less verbose way
            //user.ActAndAssert(
            //     act => act.Type($"lighthouse inspect --what logs --where {network.ResolveUri(container3)}"),
            //    (console) =>
            //    {
            //        console.Should().Contain("ping:");
            //    }
            //);

            user.ActAndAssert(
                 act => act.Type($"lighthouse stop --what ping --where {network.ResolveUri(container3)}"),
                (console) =>
                {
                    console.Should().Contain("ping stopped");
                }
            );

            user.ActAndAssert(
                 act => act.Type($"lighthouse inspect --what services --where {network.ResolveUri(container3)}"),
                consoleMultiLine: (console) =>
                {
                    console.Any(s => s.Contains("ping")).Should().BeFalse();
                }
            );

            
            user.ActAndAssert(
                 act => act.Type($"lighthouse run -what function --where {network.ResolveUri(container3)}"),
                consoleMultiLine: (console) =>
                {
                    console.Any(s => s.Contains("ping")).Should().BeFalse();
                }
            );

            // var warehouseConfig = WarehouseConfig();
            // var serializedConfig = warehouseConfig.SerializeToJSON();
            var serializedConfig = new WarehouseStoreRequest
            {
                Key = "",
                Value = ""
            };


            user.ActAndAssert(
                 act => act.Type($"lighthouse run -what store --where {network.ResolveUri(container3)} --how \"{serializedConfig}\""),
                consoleMultiLine: (console) =>
                {
                    console.Any(s => s.Contains("ping")).Should().BeFalse();
                }
            );



            #region Logging
            Output.WriteLine("Entire command line: ");
            foreach (var message in user.ConsoleLog)
                Output.WriteLine(message);
            #endregion
        }
    }

    internal class User
    {
        private readonly Action<string> WriteDelegate;
        private readonly Func<string> ReadDelegate;
        public CommandLineRunner Runner { get; }
        public VirtualNetwork Network { get; }
        public TypeFactory TypeFactory { get; }
        private string currentConsole { get; set; }
        private List<string> consoleLog { get; set; } = new List<string>();
        public IReadOnlyCollection<string> ConsoleLog => consoleLog;
        
        public User(Action<string> writeDelegate, Func<string> readDelegate, VirtualNetwork network)
        {
            WriteDelegate = writeDelegate;
            ReadDelegate = readDelegate;
            Network = network;
            TypeFactory = new TypeFactory();
            TypeFactory.Register<INetworkProvider>(() => Network);
            Runner = new CommandLineRunner((text) => {
                WriteDelegate(text);
                currentConsole = text;
                ((List<string>)ConsoleLog).Add(text); // this is SUPPER hacky....just create  backing field for the list
            }, ReadDelegate, TypeFactory);
        }

        public void Type(string text)
        {
            var message = $"USER: {text}";
            WriteDelegate(message);
            ((List<string>)ConsoleLog).Add(message);

            Runner.Run(text.Split(" ").Skip(1));
        }

        public string Read() => ReadDelegate();

        internal void ActAndAssert(Action<User> textToEnter, Action<string> console = null, Action<string[]> consoleMultiLine = null)
        {
            textToEnter(this);

            if(consoleMultiLine != null)
            {
                consoleMultiLine(currentConsole.Split(Environment.NewLine));
            }
            else if (console != null)
            {
                console(currentConsole);
            }
            else
            {
                // no op
            }
        }
    }
}
