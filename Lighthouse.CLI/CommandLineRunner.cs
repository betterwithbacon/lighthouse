using System;
using System.IO;
using CommandLine;
using Lighthouse.Client;
using Lighthouse.Core;
using Lighthouse.Core.Hosting;
using Lighthouse.Core.Utils;
using Lighthouse.Server;
using static Lighthouse.CLI.Program;
using System.Linq;
using System.Collections.Generic;
using Lighthouse.Core.IO;

namespace Lighthouse.CLI
{
    public class CommandLineRunner
    {
        public CommandLineRunner(Action<string> consoleWrite, Func<string> consoleRead, INetworkProvider networkProvider = null)
        { 
            ConsoleWrite = consoleWrite;
            ConsoleRead = consoleRead;
            NetworkProvider = networkProvider;
        }

        public Action<string> ConsoleWrite { get; }
        public Func<string> ConsoleRead { get; }
        public INetworkProvider NetworkProvider { get; private set; }

        public int Run(IEnumerable<string> args)
        {
            var result = Parser.Default.ParseArguments<RunOptions, InspectOptions>(args)
            .MapResult(
                (RunOptions run) =>
                {
                    if (run.IsFileMode)
                    {
                        //if (run.PrintOnly)
                        //{
                        //    ConsoleWrite($"Reading file contents from {run.File}.");
                        //}
                        //else
                        //{
                            var fileContents = File.ReadAllText(run.File);

                            var config = YamlUtil.ParseYaml<LighthouseRunConfig>(fileContents);

                            // load resources first
                            foreach (var resource in config.Resources)
                            {

                            }

                            // then launch apps
                            foreach (var app in config.Applications)
                            {

                            }
                        //}
                    }
                    else
                    {
                        if (run.Where != null)
                        {
                            var client = new LighthouseClient(run.Where.ToUri(), NetworkProvider);
                            client.AddLogger(ConsoleWrite);
                            
                            // TODO: I HATE this, but just want to get tests working first.
                            //if (run.PrintOnly)
                            //{
                            //    ConsoleWrite($"Request will be made to {client}");
                            //}
                            //else
                            //{
                                // make a connection to the other serer
                                var response = client.MakeRequest<RemoteAppRunRequest, RemoteAppRunHandle>(new RemoteAppRunRequest(run.What)).GetAwaiter().GetResult();
                                ConsoleWrite($"Request {response.Status} (ID: {response.Id})");
                            //}
                        }
                        else
                        {
                            Type appType = LighthouseFetcher.Fetch(run.What);
                            if (appType == null)
                            {
                                throw new ApplicationException($"Can't find app with name: {run.What}");
                            }

                            // start a lighthouse server locally, and have it run the task
                            var server = new LighthouseServer();
                            server.AddLogger(ConsoleWrite);
                            server.Launch(appType).GetAwaiter().GetResult();
                        }
                    }

                    return 0;
                },
                (InspectOptions inspect) =>
                {
                    var client = new LighthouseClient(inspect.Where.ToUri(), NetworkProvider);
                    client.AddLogger(ConsoleWrite);

                    // this REQUIRES a local
                    if (inspect.Where == null)
                        throw new Exception("Must include Where to inspect.");

                    var response = client.MakeRequest<StatusRequest, StatusResponse>(new StatusRequest()).GetAwaiter().GetResult();
                    ConsoleWrite("");
                    return 0;
                },
                errs =>
                {
                    foreach(var error in errs)
                    {
                        ConsoleWrite(error.ToString());
                    }
                    return errs.Count();
                }); 

            return 0;
        }
    }
}
