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
        public CommandLineRunner(Action<string> consoleWrite, Func<string> consoleRead,
            TypeFactory typeFactory = null)
        { 
            ConsoleWrite = consoleWrite;
            ConsoleRead = consoleRead;
            if(typeFactory== null)
            {
                typeFactory = new TypeFactory();
                typeFactory.Register<INetworkProvider>(() => new InternetNetworkProvider());
            }
            else
            {
                TypeFactory = typeFactory;
            }            
        }

        public Action<string> ConsoleWrite { get; }
        public Func<string> ConsoleRead { get; }
        public TypeFactory TypeFactory { get; }
        
        public int Run(IEnumerable<string> args)
        {
            var result = Parser.Default.ParseArguments<RunOptions, InspectOptions>(args)
            .MapResult(
                (RunOptions run) =>
                {
                    if (run.IsFileMode)
                    {
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
                    }
                    else
                    {
                        if (run.Where != null)
                        {
                            var networkProvider = TypeFactory.Create<INetworkProvider>();
                            var client = new LighthouseClient(run.Where.ToUri(), networkProvider);
                            client.AddLogger(ConsoleWrite);
                            
                            // make a connection to the other server
                            var response = client.HandleRequest<RemoteAppRunRequest, RemoteAppRunHandle>(new RemoteAppRunRequest(run.What)).GetAwaiter().GetResult();
                            ConsoleWrite($"Request {response?.Status ?? "no response"} (ID: {response?.Id ?? "no ID"})");
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
                    var networkProvider = TypeFactory.Create<INetworkProvider>();
                    var client = new LighthouseClient(inspect.Where.ToUri(), networkProvider);
                    client.AddLogger(ConsoleWrite);

                    // this REQUIRES a local
                    if (inspect.Where == null)
                        throw new Exception("Must include Where to inspect.");

                    var response = client.HandleRequest<StatusRequest, StatusResponse>(new StatusRequest()).GetAwaiter().GetResult();
                    ConsoleWrite(response.ToString());
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
